using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using CosmoStudio.BLL.Clientes;
using CosmoStudio.BLL.Servicios.Interfaces;
using CosmoStudio.Common;
using CosmoStudio.Common.Opciones;
using CosmoStudio.Infraestructura.DAL.Repos.Interfaces;
using CosmoStudio.Model;
using Microsoft.Extensions.Options;

public sealed class AudioServicio : IAudioServicio
{
    private readonly IGuionServicio _guiones;
    private readonly IGuionAudioRepositorio _guionAudio;
    private readonly IRecursoServicio _recursos;
    private readonly IkokoroClient _kokoroClient;
    private readonly KokoroOptions _kokoroOptions;
    private readonly ILocalFileStorage _fileStorage; // <- nuevo

    public AudioServicio(
        IGuionServicio guiones,
        IRecursoServicio recursos,
        IkokoroClient kokoroClient,
        IOptions<KokoroOptions> kokoroOptions,
        IOptions<StorageOptions> storageOptions, 
        IGuionAudioRepositorio guionAudio,
        ILocalFileStorage fileStorage 
    )
    {
        _guiones = guiones;
        _recursos = recursos;
        _kokoroClient = kokoroClient;
        _kokoroOptions = kokoroOptions.Value;
        _guionAudio = guionAudio;
        _fileStorage = fileStorage;
    }

    /// <summary>
    /// Compatibilidad: genera por secciones y devuelve el último recurso creado.
    /// </summary>
    public async Task<long> GenerarVozDesdeGuionAsync(long idProyecto, CancellationToken ct = default)
    {
        var ids = await GenerarVozDesdeGuionPorSeccionesAsync(idProyecto, ct);
        return ids.Count > 0 ? ids[^1] : 0L;
    }

    /// <summary>
    /// Procesa el guion por secciones y crea un audio por sección.
    /// Devuelve los IDs de recursos creados en orden de aparición.
    /// </summary>
    public async Task<IReadOnlyList<long>> GenerarVozDesdeGuionPorSeccionesAsync(long idProyecto, CancellationToken ct = default)
    {
        // 1) Guion + versión vigente
        var guion = await _guiones.ObtenerPorProyectoAsync(idProyecto, ct)
            ?? throw new InvalidOperationException($"Proyecto {idProyecto} no tiene guion.");
        var version = guion.CurrentVersion ?? throw new InvalidOperationException("El guion no tiene versión vigente.");
        var scriptPath = version.ScriptRecurso?.StoragePath
            ?? throw new InvalidOperationException("La versión vigente no tiene Script asociado.");

        var markdown = await File.ReadAllTextAsync(scriptPath, ct);

        // 2) Secciones
        var secciones = ExtraerSecciones(markdown);
        if (secciones.Count == 0)
            throw new InvalidOperationException("No se encontraron secciones (## ...) en el guion.");

        // 3) Extensión de salida
        var ext = _kokoroOptions.Format?.Equals("mp3", StringComparison.OrdinalIgnoreCase) == true ? "mp3" : "wav";

        // IMPORTANTE: número de versión para carpetas/nombres.
        // Usamos el Id de la versión como entero; si prefieres otro campo (p. ej. version.Number),
        // cambia aquí el mapeo.
        var versionNumber = version.NumeroVersion;

        var recursoIds = new List<long>(secciones.Count);

        // 4) Sintetizar y guardar por sección
        for (int i = 0; i < secciones.Count; i++)
        {
            ct.ThrowIfCancellationRequested();

            var s = secciones[i];
            var textoLimpio = LimpiarTextoParaKokoro(s.Cuerpo);
            if (string.IsNullOrWhiteSpace(textoLimpio))
                continue;

            // 4.1) TTS
            var audioBytes = await _kokoroClient.SynthesizeAsync(textoLimpio, ct: ct);

            // 4.2) Guardar con LocalFileStorage (se encarga de ruta y nombre)
            // Nombre final: {TipoRecurso}_v{version}_{index}.{ext}
            var storagePath = await _fileStorage.SaveBytesAsync(
                idProyecto: idProyecto,
                version: versionNumber,
                tipo: TipoRecurso.Voz,
                data: audioBytes,
                extension: ext,
                index: i, 
                ct: ct
            );

            // 4.3) Registrar Recurso (Audio/Voz) con metadatos
            var meta = JsonSerializer.Serialize(new
            {
                _kokoroOptions.Voice,
                _kokoroOptions.Model,
                _kokoroOptions.Format,
                SectionIndex = i,
                SectionTitle = s.Titulo,
                Timestamp = s.Timestamp,
                TextLength = textoLimpio.Length
            });

            var recurso = await _recursos.AgregarAsync(idProyecto, TipoRecurso.Voz, storagePath, meta, ct);
            recursoIds.Add(recurso.Id);

            // 4.4) Asociar a versión con orden
            await _guionAudio.AgregarAsync(new GuionAudio
            {
                IdGuionVersion = version.Id,
                IdAudioRecurso = recurso.Id,
                Orden = await _guionAudio.ObtenerSiguienteOrdenAsync(version.Id, ct)
            }, ct);
            await _guionAudio.GuardarCambiosAsync(ct);
        }

        return recursoIds;
    }

    // ======== Helpers ========

    private sealed record Seccion(string? Titulo, string? Timestamp, string Cuerpo);

    /// <summary>Busca cabeceras "## Titulo  —  ~[mm:ss]" y devuelve secciones con sus cuerpos.</summary>
    private static List<Seccion> ExtraerSecciones(string markdown)
    {
        var headerRx = new Regex(
            @"^##\s*(.+?)\s*(?:—\s*~\[(\d{2}:\d{2})\])?\s*$",
            RegexOptions.Multiline | RegexOptions.CultureInvariant);

        var matches = headerRx.Matches(markdown);
        var secciones = new List<Seccion>(matches.Count);
        if (matches.Count == 0) return secciones;

        for (int i = 0; i < matches.Count; i++)
        {
            var m = matches[i];
            var startOfBody = m.Index + m.Length;
            var endOfBody = (i + 1 < matches.Count) ? matches[i + 1].Index : markdown.Length;
            var rawBody = markdown.Substring(startOfBody, endOfBody - startOfBody);

            var cuerpo = rawBody.Trim('\r', '\n', ' ');
            var titulo = m.Groups[1].Value?.Trim();
            var timestamp = m.Groups[2].Success ? m.Groups[2].Value : null;

            secciones.Add(new Seccion(titulo, timestamp, cuerpo));
        }

        return secciones;
    }

    /// <summary>Reemplaza "(pausa breve)" -> "[...]\n\n" y normaliza saltos de línea.</summary>
    private static string LimpiarTextoParaKokoro(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;

        var pausaRx = new Regex(@"\(\s*pausa\s+breve\s*\)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        var replaced = pausaRx.Replace(input, "[...]\n\n");

        replaced = replaced.Replace("\r\n", "\n");
        replaced = Regex.Replace(replaced, @"[ \t]+\n", "\n");
        replaced = Regex.Replace(replaced, @"\n{3,}", "\n\n");

        return replaced.Trim();
    }
}
