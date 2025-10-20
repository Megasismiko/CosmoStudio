using CosmoStudio.BLL.Ollama;
using CosmoStudio.BLL.Servicios.Interfaces;
using CosmoStudio.Common;
using CosmoStudio.Infraestructura.DAL.Repos.Interfaces;
using CosmoStudio.Model;
using Microsoft.Extensions.Options;


namespace CosmoStudio.BLL.Servicios.Implementaciones;

public class GuionServicio : IGuionServicio
{
    private readonly IGuionRepositorio _repo;
    private readonly IProyectoRepositorio _proyectos;
    private readonly IOllamaClient _llm;
    private readonly IFileStorage _files;
    private readonly StorageOptions _storage;

    public GuionServicio(
        IGuionRepositorio repo,
        IProyectoRepositorio proyectos,
        IOllamaClient llm,
        IFileStorage files,
        IOptions<StorageOptions> storage
    )
    {
        _repo = repo;
        _proyectos = proyectos;
        _llm = llm;
        _files = files;
        _storage = storage.Value;
    }

    public Task<Guion?> ObtenerPorProyectoAsync(long idProyecto, CancellationToken ct) =>
        _repo.ObtenerPorProyectoAsync(idProyecto, ct);

    public async Task<Guion> GuardarRutasAsync(long idProyecto, string rutaOutline, string rutaCompleto, int version, CancellationToken ct)
    {
        var proyecto = await _proyectos.ObtenerPorIdAsync(idProyecto, ct)
            ?? throw new InvalidOperationException("Proyecto no encontrado");

        var existente = await _repo.ObtenerPorProyectoAsync(idProyecto, ct);
        if (existente is null)
        {
            var g = new Guion
            {
                IdProyecto = proyecto.Id,
                RutaOutline = rutaOutline,
                RutaCompleto = rutaCompleto,
                Version = version <= 0 ? 1 : version
            };
            await _repo.CrearAsync(g, ct);
            await _repo.GuardarCambiosAsync(ct);
            return g;
        }
        else
        {
            existente.RutaOutline = rutaOutline;
            existente.RutaCompleto = rutaCompleto;
            existente.Version = version <= 0 ? existente.Version : version;
            await _repo.ActualizarAsync(existente, ct);
            await _repo.GuardarCambiosAsync(ct);
            return existente;
        }
    }

    public async Task<bool> EliminarPorProyectoAsync(long idProyecto, CancellationToken ct)
    {
        await _repo.EliminarPorProyectoAsync(idProyecto, ct);
        await _repo.GuardarCambiosAsync(ct);
        return true;
    }

    public async Task<(string outlinePath, string outline)> GenerarOutlineAsync(long idProyecto, ScriptGenOptions opt, CancellationToken ct)
    {
        var proyecto = await _proyectos.ObtenerPorIdAsync(idProyecto, ct)
            ?? throw new InvalidOperationException("Proyecto no encontrado");

        var outline = await _llm.GenerateOutlineAsync(proyecto.Tema, opt, ct);

        var runDir =  Path.Combine(_storage.RunsRoot, idProyecto.ToString());
        await _files.EnsureDirectoryAsync(runDir, ct);
        var outlinePath = Path.Combine(runDir, "script_outline.md");
        await _files.WriteAllTextAsync(outlinePath, outline, ct);

        var g = await _repo.ObtenerPorProyectoAsync(proyecto.Id, ct);
        if (g is null)
            await GuardarRutasAsync(proyecto.Id, outlinePath, "", 1, ct);
        else
            await GuardarRutasAsync(proyecto.Id, outlinePath, g.RutaCompleto, g.Version + 1, ct);

        return (outlinePath, outline);
    }

    public async Task<(string scriptPath, string script)> GenerarGuionDesdeOutlineAsync(long idProyecto, ScriptGenOptions opt, CancellationToken ct)
    {
        var proyecto = await _proyectos.ObtenerPorIdAsync(idProyecto, ct)
            ?? throw new InvalidOperationException("Proyecto no encontrado");

        var g = await _repo.ObtenerPorProyectoAsync(proyecto.Id, ct)
            ?? throw new InvalidOperationException("No existe outline para este proyecto");

        var outline = await File.ReadAllTextAsync(g.RutaOutline, ct);
        var script = await _llm.GenerateScriptFromOutlineAsync(outline, opt, ct);

        var runDir = Path.Combine(_storage.RunsRoot, idProyecto.ToString());
        var scriptPath = Path.Combine(runDir, "script_full.md");
        await _files.WriteAllTextAsync(scriptPath, script, ct);

        await GuardarRutasAsync(proyecto.Id, g.RutaOutline, scriptPath, g.Version + 1, ct);
        return (scriptPath, script);
    }


    public async Task<(string scriptPath, string script)> GenerarGuionDesdeOutlinePorSeccionesAsync(
    long idProyecto, ScriptGenOptions opt, CancellationToken ct)
    {
        var proyecto = await _proyectos.ObtenerPorIdAsync(idProyecto, ct)
            ?? throw new InvalidOperationException("Proyecto no encontrado");

        var g = await _repo.ObtenerPorProyectoAsync(proyecto.Id, ct)
            ?? throw new InvalidOperationException("No existe outline para este proyecto");

        var outline = await File.ReadAllTextAsync(g.RutaOutline, ct);

        // 1) Parsear secciones del outline
        var sections = ParseOutline(outline); // ver método abajo
        if (sections.Count == 0) throw new InvalidOperationException("Outline sin secciones parseables");

        // 2) Calcular objetivo por sección
        var wpm = Math.Clamp(opt.PalabrasPorMinuto, 90, 180);
        var totalWords = opt.MinutosObjetivo * wpm;
        var wordsPerSection = Math.Max(120, totalWords / Math.Max(1, sections.Count));

        // 3) Escribir incrementalmente
        var runDir = Path.Combine(_storage.RunsRoot, idProyecto.ToString());
        Directory.CreateDirectory(runDir);
        var scriptPath = Path.Combine(runDir, "script_full.md");
        var fs = new FileStream(scriptPath, FileMode.Create, FileAccess.Write, FileShare.Read);
        await using var sw = new StreamWriter(fs);

        await sw.WriteLineAsync($"# {proyecto.Titulo}");
        await sw.WriteLineAsync();

        var minuto = 0;
        foreach (var s in sections)
        {
            ct.ThrowIfCancellationRequested();
            var texto = await _llm.GenerateSectionAsync(s.Title, s.Bullets, wordsPerSection, opt, ct);

            // Marca de tiempo aproximada por sección (opcional)
            await sw.WriteLineAsync($"## {s.Title}  —  ~[{minuto:00}:00]");
            await sw.WriteLineAsync(texto.Trim());
            await sw.WriteLineAsync();
            minuto += Math.Max(1, opt.MinutosObjetivo / Math.Max(1, sections.Count));
        }

        await sw.FlushAsync();
        await sw.DisposeAsync();

        // 4) Guardar ruta en BD
        await GuardarRutasAsync(proyecto.Id, g.RutaOutline, scriptPath, g.Version + 1, ct);

        var final = await ReadAllTextWithRetryAsync(scriptPath);
        return (scriptPath, final);
    }


    static async Task<string> ReadAllTextWithRetryAsync(string path, int retries = 3, int delayMs = 150)
    {
        for (int i = 0; i <= retries; i++)
        {
            try
            {
                using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var sr = new StreamReader(fs);
                return await sr.ReadToEndAsync();
            }
            catch (IOException) when (i < retries)
            {
                await Task.Delay(delayMs);
            }
        }
        // último intento sin capturar para ver el error real
        using var fs2 = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var sr2 = new StreamReader(fs2);
        return await sr2.ReadToEndAsync();
    }
    // --- Parser simple de outline ---
    private static List<(string Title, List<string> Bullets)> ParseOutline(string outline)
    {
        var result = new List<(string, List<string>)>();
        var lines = outline.Split('\n').Select(l => l.Trim()).ToList();

        string? currentTitle = null;
        var currentBullets = new List<string>();

        foreach (var line in lines)
        {
            // detecta "1) Título", "12) Título", o "1. Título"
            if (System.Text.RegularExpressions.Regex.IsMatch(line, @"^\d+[\)\.]\s"))
            {
                if (currentTitle != null)
                    result.Add((currentTitle, new List<string>(currentBullets)));

                currentTitle = System.Text.RegularExpressions.Regex.Replace(line, @"^\d+[\)\.]\s*", "");
                currentBullets.Clear();
            }
            else if (line.StartsWith("- "))
            {
                currentBullets.Add(line.Substring(2));
            }
        }
        if (currentTitle != null)
            result.Add((currentTitle, currentBullets));

        return result;
    }

}
