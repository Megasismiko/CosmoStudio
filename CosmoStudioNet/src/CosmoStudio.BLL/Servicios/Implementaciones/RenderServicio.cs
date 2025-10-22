using CosmoStudio.BLL.Servicios.Interfaces;
using CosmoStudio.Common;
using System.Diagnostics;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;

public sealed class RenderServicio : IRenderServicio
{
    private readonly IGuionServicio _guiones;
    private readonly ILocalFileStorage _storage;

    // Ajusta estas rutas si prefieres usar PATH en vez de absoluto.
    private const string FFPROBE = @"C:\ffmpeg\bin\ffprobe.exe";
    private const string FFMPEG = @"C:\ffmpeg\bin\ffmpeg.exe";

    public RenderServicio(IGuionServicio guiones, ILocalFileStorage storage)
    {
        _guiones = guiones;
        _storage = storage;
    }

    // === 1) GENERAR MANIFEST con UTF-8 legible ===
    public async Task<(string manifestPath, RenderManifest manifest)> GenerarManifestAsync(
        long idProyecto, int fps = 30, string resolution = "1920x1080", CancellationToken ct = default)
    {
        // 1) Guion + versión vigente
        var guion = await _guiones.ObtenerPorProyectoAsync(idProyecto, ct)
            ?? throw new InvalidOperationException($"Proyecto {idProyecto} no tiene guion.");
        var version = guion.CurrentVersion ?? throw new InvalidOperationException("No hay versión vigente.");
        var versionNumber = version.NumeroVersion;

        var scriptPath = version.ScriptRecurso?.StoragePath
            ?? throw new InvalidOperationException("La versión vigente no tiene Script asociado.");
        var markdown = await File.ReadAllTextAsync(scriptPath, ct);

        // 2) Secciones del guion
        var secciones = ExtraerSecciones(markdown);

        // 3) Localizar assets por convención
        var audioDir = _storage.GetResourceDirectory(idProyecto, versionNumber, TipoRecurso.Voz);
        var imageDir = _storage.GetResourceDirectory(idProyecto, versionNumber, TipoRecurso.Imagen);

        var audioMap = EnumerateByIndex(audioDir, $"{nameof(TipoRecurso.Voz)}_v{versionNumber}_*.wav");
        var imgMap = EnumerateByIndex(imageDir, $"{nameof(TipoRecurso.Imagen)}_v{versionNumber}_*.png");

        // 4) Construir manifest
        var manifest = new RenderManifest
        {
            ProjectId = idProyecto,
            Version = versionNumber,
            Fps = fps,
            Resolution = resolution,
            Sections = new List<RenderSection>(secciones.Count)
        };

        for (int i = 0; i < secciones.Count; i++)
        {
            ct.ThrowIfCancellationRequested();

            audioMap.TryGetValue(i, out var audioPath);
            imgMap.TryGetValue(i, out var imagePath);

            // Duración de audio (segundos) si existe
            double? duration = null;
            if (!string.IsNullOrWhiteSpace(audioPath) && File.Exists(audioPath))
                duration = await GetAudioDurationAsync(audioPath!, ct);

            manifest.Sections.Add(new RenderSection
            {
                Index = i,
                Title = secciones[i].Titulo ?? $"Section {i}",
                Text = LimpiarTexto(secciones[i].Cuerpo),
                Audio = audioPath,
                Image = imagePath,
                Duration = duration,
                TransitionIn = new Transition { Type = "fade", Duration = 0.5 },
                TransitionOut = new Transition { Type = "fade", Duration = 0.5 },
                KenBurns = new KenBurns { Mode = "auto", Zoom = 1.06, Pan = (i % 2 == 0) ? "right" : "left" },
                OverlayTitle = new OverlayTitle { Enabled = true, Title = secciones[i].Titulo, ShowAt = 0.5, Duration = 2.0 }
            });
        }

        // 5) Guardar manifest en video/ con UTF-8 sin \uXXXX
        var json = JsonSerializer.Serialize(
            manifest,
            new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });

        var manifestPath = await _storage.SaveTextAsync(
            idProyecto: idProyecto,
            version: versionNumber,
            tipo: TipoRecurso.Video,
            content: json,
            extension: "json",
            index: null,
            ct: ct);

        return (manifestPath, manifest);
    }

    // === 2) RENDERIZAR VIDEO DESDE PROYECTO (usa manifest por convención) ===
    public async Task<string> RenderProyectoVigenteAsync(long idProyecto, CancellationToken ct = default)
    {
        var guion = await _guiones.ObtenerPorProyectoAsync(idProyecto, ct)
            ?? throw new InvalidOperationException("Proyecto sin guion");
        var v = guion.CurrentVersion ?? throw new InvalidOperationException("Sin versión vigente");

        // Genera/actualiza manifest y renderiza
        var (manifestPath, _) = await GenerarManifestAsync(idProyecto, ct: ct);
        return await RenderDesdeManifestAsync(manifestPath, ct);
    }

    // === 3) RENDERIZAR VIDEO DESDE UN MANIFEST ===
    public async Task<string> RenderDesdeManifestAsync(string manifestPath, CancellationToken ct = default)
    {
        if (!File.Exists(manifestPath))
            throw new FileNotFoundException("No se encontró el manifest", manifestPath);

        var json = await File.ReadAllTextAsync(manifestPath, ct);
        var manifest = JsonSerializer.Deserialize<RenderManifest>(json)
            ?? throw new InvalidOperationException("Manifest inválido");

        var idProyecto = manifest.ProjectId;
        var version = manifest.Version;
        var fps = manifest.Fps;
        var (w, h) = ParseRes(manifest.Resolution);

        var videoDir = _storage.GetResourceDirectory(idProyecto, version, TipoRecurso.Video);
        Directory.CreateDirectory(videoDir);

        // Intermedios y lista de concat
        var intermediatesDir = Path.Combine(videoDir, "intermediate");
        Directory.CreateDirectory(intermediatesDir);

        var listFile = Path.Combine(videoDir, $"concat_v{version}.txt");
        var listSb = new StringBuilder();

        for (int i = 0; i < manifest.Sections.Count; i++)
        {
            ct.ThrowIfCancellationRequested();
            var s = manifest.Sections[i];

            if (string.IsNullOrWhiteSpace(s.Image) || !File.Exists(s.Image))
                continue; // sin imagen, no renderizamos sección

            // Duración (audio > manifest.Duration > fallback 6s)
            double duration = 6.0;
            if (!string.IsNullOrWhiteSpace(s.Audio) && File.Exists(s.Audio))
            {
                duration = await GetAudioDurationAsync(s.Audio!, ct) ?? 6.0;
            }
            else if (s.Duration is double d && d > 0) duration = d;

            var outPath = Path.Combine(intermediatesDir, $"section_{i:00}.mp4");

            var hasAudio = !string.IsNullOrWhiteSpace(s.Audio) && File.Exists(s.Audio);
            var args = hasAudio
                ? $"-y -loop 1 -i \"{s.Image}\" -i \"{s.Audio}\" -t {duration.ToString(System.Globalization.CultureInfo.InvariantCulture)} " +
                  $"-lavfi \"[0:v]scale={w}:{h}:force_original_aspect_ratio=decrease,pad={w}:{h}:(ow-iw)/2:(oh-ih)/2,format=yuv420p[v]\" " +
                  $"-map \"[v]\" -map 1:a -r {fps} -c:v libx264 -preset medium -crf 18 -c:a aac -b:a 192k \"{outPath}\""
                : $"-y -loop 1 -i \"{s.Image}\" -t {duration.ToString(System.Globalization.CultureInfo.InvariantCulture)} " +
                  $"-vf \"scale={w}:{h}:force_original_aspect_ratio=decrease,pad={w}:{h}:(ow-iw)/2:(oh-ih)/2,format=yuv420p\" " +
                  $"-r {fps} -c:v libx264 -preset medium -crf 18 -an \"{outPath}\"";

            await RunProcessAsync(FFMPEG, args, ct);
            listSb.AppendLine($"file '{outPath.Replace("'", "'\\''")}'");
        }

        // Concatenar
        await File.WriteAllTextAsync(listFile, listSb.ToString(), ct);
        var finalPath = Path.Combine(videoDir, $"Render_v{version}.mp4");
        var concatArgs = $"-y -f concat -safe 0 -i \"{listFile}\" -c:v libx264 -preset medium -crf 18 -c:a aac -b:a 192k \"{finalPath}\"";
        await RunProcessAsync(FFMPEG, concatArgs, ct);

        return finalPath;
    }

    // ===== Helpers =====

    private static Dictionary<int, string> EnumerateByIndex(string dir, string pattern)
    {
        var map = new Dictionary<int, string>();
        if (!Directory.Exists(dir)) return map;

        foreach (var path in Directory.EnumerateFiles(dir, pattern))
        {
            var name = Path.GetFileNameWithoutExtension(path);
            var m = Regex.Match(name, @"_(\d{2})$");
            if (m.Success && int.TryParse(m.Groups[1].Value, out var idx))
                map[idx] = path;
        }
        return map;
    }

    private sealed record Seccion(string? Titulo, string? Timestamp, string Cuerpo);

    private static List<Seccion> ExtraerSecciones(string markdown)
    {
        var rx = new Regex(@"^##\s*(.+?)\s*(?:—\s*~\[(\d{2}:\d{2})\])?\s*$",
            RegexOptions.Multiline | RegexOptions.CultureInvariant);

        var matches = rx.Matches(markdown);
        var list = new List<Seccion>(matches.Count);
        if (matches.Count == 0) return list;

        for (int i = 0; i < matches.Count; i++)
        {
            var m = matches[i];
            var start = m.Index + m.Length;
            var end = (i + 1 < matches.Count) ? matches[i + 1].Index : markdown.Length;
            var rawBody = markdown.Substring(start, end - start);
            list.Add(new Seccion(
                m.Groups[1].Value.Trim(),
                m.Groups[2].Success ? m.Groups[2].Value : null,
                rawBody.Trim()));
        }
        return list;
    }

    private static string LimpiarTexto(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;
        var t = Regex.Replace(input, @"\(\s*pausa\s+breve\s*\)", "[...]\n", RegexOptions.IgnoreCase);
        t = Regex.Replace(t, @"^#{1,6}\s+.*$", "", RegexOptions.Multiline);
        t = Regex.Replace(t, @"^\s*[-*+]\s+", "", RegexOptions.Multiline);
        t = Regex.Replace(t, @"^\s*\d+\.\s+", "", RegexOptions.Multiline);
        t = t.Replace("\r\n", "\n");
        t = Regex.Replace(t, @"[ \t]+\n", "\n");
        t = Regex.Replace(t, @"\n{3,}", "\n\n");
        return t.Trim();
    }

    private static async Task<double?> GetAudioDurationAsync(string audioPath, CancellationToken ct)
    {
        var psi = new ProcessStartInfo
        {
            FileName = FFPROBE,
            Arguments = $"-v error -show_entries format=duration -of default=nk=1:nw=1 \"{audioPath}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var p = new Process { StartInfo = psi };
        p.Start();
        var output = await p.StandardOutput.ReadToEndAsync();
        await p.WaitForExitAsync(ct);

        if (double.TryParse(output.Trim(), System.Globalization.CultureInfo.InvariantCulture, out var sec))
            return sec;

        return null;
    }

    private static async Task RunProcessAsync(string fileName, string args, CancellationToken ct)
    {
        var psi = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = args,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var p = new Process { StartInfo = psi };
        p.Start();

        // opcional: logs
        _ = p.StandardOutput.ReadToEndAsync();
        var err = await p.StandardError.ReadToEndAsync();
        await p.WaitForExitAsync(ct);

        if (p.ExitCode != 0)
            throw new InvalidOperationException($"{Path.GetFileName(fileName)} falló.\nArgs: {args}\n{err}");
    }

    private static (int w, int h) ParseRes(string res)
    {
        var parts = res.ToLowerInvariant().Split('x', '×');
        if (parts.Length == 2 &&
            int.TryParse(parts[0], out var w) &&
            int.TryParse(parts[1], out var h))
            return (w, h);
        return (1920, 1080);
    }
}

