using CosmoStudio.BLL.Clientes;
using CosmoStudio.BLL.Servicios.Interfaces;
using CosmoStudio.Common;
using CosmoStudio.Common.Opciones;
using CosmoStudio.Common.Requests;
using CosmoStudio.Infraestructura.DAL.Repos.Interfaces;
using CosmoStudio.Model;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace CosmoStudio.BLL.Servicios.Implementaciones;

public sealed class ImagenServicio : IImagenServicio
{
    private readonly IStableDifusionClient _stableClient;
    private readonly IOllamaClient _ollama;
    private readonly IRecursoServicio _recursos;
    private readonly IGuionServicio _guiones;
    private readonly ILocalFileStorage _fileStorage;
    private readonly IGuionImagenRepositorio _guionImagen;    
    private readonly StableDiffusionOptions _sdOptions;

    public ImagenServicio(
        IStableDifusionClient stableClient,
        IRecursoServicio recursos,
        IGuionServicio guiones,
        ILocalFileStorage fileStorage,
        IOptions<StableDiffusionOptions> sdOptions,
        IOllamaClient ollama,
        IGuionImagenRepositorio guionImagen)
    {
        _stableClient = stableClient;
        _recursos = recursos;
        _guiones = guiones;
        _fileStorage = fileStorage;
        _sdOptions = sdOptions.Value;
        _ollama = ollama;
        _guionImagen = guionImagen;
    }

    /// <summary>
    /// Genera 1 imagen por sección del guion vigente. Todo desde configuración (sin parámetros de ajuste).
    /// </summary>
    public async Task<IReadOnlyList<long>> GenerarImagenesDesdeGuionPorSeccionesAsync(long idProyecto, CancellationToken ct = default)
    {
        // 1) Guion y versión
        var guion = await _guiones.ObtenerPorProyectoAsync(idProyecto, ct)
            ?? throw new InvalidOperationException($"Proyecto {idProyecto} no tiene guion.");
        var version = guion.CurrentVersion ?? throw new InvalidOperationException("El guion no tiene versión vigente.");
        var scriptPath = version.ScriptRecurso?.StoragePath
            ?? throw new InvalidOperationException("La versión vigente no tiene Script asociado.");
        var markdown = await File.ReadAllTextAsync(scriptPath, ct);

        // 2) Extraer secciones
        var secciones = ExtraerSecciones(markdown);
        if (secciones.Count == 0)
            throw new InvalidOperationException("No se encontraron secciones (## ...) en el guion.");

        var versionNumber = version.NumeroVersion;
        var recursoIds = new List<long>(secciones.Count);

        // 3) Configuración (solo appsettings)
        var width = _sdOptions.Width > 0 ? _sdOptions.Width : 960;
        var height = _sdOptions.Height > 0 ? _sdOptions.Height : 540;

        for (int i = 0; i < secciones.Count; i++)
        {
            ct.ThrowIfCancellationRequested();

            var s = secciones[i];
            var englishPrompt = await _ollama.GenerarPromptImagenDesdeSeccionAsync(s.Cuerpo, ct);
            
            var prompt = $"{englishPrompt}, ultra detailed, realistic lighting, 8k, photo,studio";
            prompt = MinifyPrompt(prompt);

            if (string.IsNullOrWhiteSpace(prompt))
                continue;

            var req = new Txt2ImgRequest
            {
                Prompt = prompt,
                NegativePrompt = _sdOptions.DefaultNegativePrompt,
                Steps = _sdOptions.Steps,
                CfgScale = _sdOptions.CfgScale,
                Width = width,
                Height = height,
                SamplerName = _sdOptions.DefaultSampler,
                Scheduler = _sdOptions.DefaultScheduler,
                EnableHr = _sdOptions.EnableHiresFix,
                HrScale = _sdOptions.HiresUpscale,
                HrUpscaler = _sdOptions.Upscaler,
                DenoisingStrength = _sdOptions.DenoisingStrength,
                HiresSteps = _sdOptions.HiresSteps,
                Seed = -1
            };

            var res = await _stableClient.Txt2ImgAsync(req, ct);
            if (res.Images is null || res.Images.Count == 0)
                continue;

            var bytes = Convert.FromBase64String(res.Images.First());

            // 4) Guardar: runs/{id}/v{version}/image/Imagen_v{version}_{i:00}.png
            var storagePath = await _fileStorage.SaveBytesAsync(
                idProyecto: idProyecto,
                version: versionNumber,
                tipo: TipoRecurso.Imagen,
                data: bytes,
                extension: "png",
                index: i,
                ct: ct);

            var meta = JsonSerializer.Serialize(new
            {
                SectionIndex = i,
                SectionTitle = s.Titulo,
                Timestamp = s.Timestamp,
                Prompt = Trunc(prompt, 512),
                _sdOptions.Width,
                _sdOptions.Height,
                _sdOptions.EnableHiresFix,
                _sdOptions.HiresUpscale,
                _sdOptions.DefaultSampler,
                _sdOptions.DefaultScheduler,
                _sdOptions.CfgScale,
                _sdOptions.Steps,
                _sdOptions.Upscaler
            });

            var recurso = await _recursos.AgregarAsync(idProyecto, TipoRecurso.Imagen, storagePath, meta, ct);
            recursoIds.Add(recurso.Id);

            await _guionImagen.AgregarAsync(new GuionImagen
            {
                IdGuionVersion = version.Id,
                IdImagenRecurso = recurso.Id,                
                Orden = await _guionImagen.ObtenerSiguienteOrdenAsync(version.Id, ct)
            }, ct);
            await _guionImagen.GuardarCambiosAsync(ct);
        }

        return recursoIds;
    }

    // ===== Helpers =====

   
    private sealed record Seccion(string? Titulo, string? Timestamp, string Cuerpo);

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
            var startBody = m.Index + m.Length;
            var endBody = (i + 1 < matches.Count) ? matches[i + 1].Index : markdown.Length;
            var raw = markdown.Substring(startBody, endBody - startBody);

            var cuerpo = raw.Trim('\r', '\n', ' ');
            var titulo = m.Groups[1].Value?.Trim();
            var ts = m.Groups[2].Success ? m.Groups[2].Value : null;

            secciones.Add(new Seccion(titulo, ts, cuerpo));
        }
        return secciones;
    }

    private static string PrepararPromptDesdeCuerpo(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;

        // (pausa breve) -> "[...]\n"
        var pausaRx = new Regex(@"\(\s*pausa\s+breve\s*\)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        var text = pausaRx.Replace(input, "[...]\n");

        // limpiar markdown/viñetas
        text = Regex.Replace(text, @"^#{1,6}\s+.*$", "", RegexOptions.Multiline);
        text = Regex.Replace(text, @"^\s*[-*+]\s+", "", RegexOptions.Multiline);
        text = Regex.Replace(text, @"^\s*\d+\.\s+", "", RegexOptions.Multiline);

        // normalizar espacios/saltos
        text = text.Replace("\r\n", "\n");
        text = Regex.Replace(text, @"[ \t]+\n", "\n");
        text = Regex.Replace(text, @"\n{3,}", "\n\n");

        // recortar prompts excesivamente largos (SD rinde peor con texto kilométrico)
        return Trunc(text.Trim(), 1200);
    }

    private static string Trunc(string s, int max) => (s.Length <= max) ? s : s[..max];

    private string MinifyPrompt(string p) =>
    System.Text.RegularExpressions.Regex.Replace(p.Trim('\"', '“', '”'), @"\s{2,}", " ").Trim();
}
