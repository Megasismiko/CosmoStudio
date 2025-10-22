using System.Diagnostics;
using CosmoStudio.BLL.Servicios.Interfaces;
using CosmoStudio.Common;
using CosmoStudio.Common.Interfaces;
using CosmoStudio.Common.Requests;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/guion")]
public class GuionController : ControllerBase
{
    private readonly IGuionServicio _svc;
    private readonly IScriptGenOptionsProvider _profiles;

    public GuionController(IGuionServicio svc, IScriptGenOptionsProvider profiles)
    {
        _svc = svc;
        _profiles = profiles;
    }

    // Genera el outline (esquema) de un proyecto.
    [HttpPost("{idProyecto:long}/outline")]
    public async Task<IActionResult> GenerarOutline(
        long idProyecto,
        [FromQuery] OllamaMode mode = OllamaMode.Borrador,       
        CancellationToken ct = default)
    {
        var sw = Stopwatch.StartNew();

        var opt = BuildOptionsFromProfile(mode);
        var (path, _) = await _svc.GenerarOutlineAsync(idProyecto, opt, ct);

        sw.Stop();
        var elapsedMs = sw.ElapsedMilliseconds;

        return Ok(new
        {
            ok = true,
            outlinePath = path,
            mode = opt.Mode.ToString(),
            minutos = opt.MinutosObjetivo,
            elapsedMs
        });
    }

    // Genera el guion por secciones a partir del outline existente.
    [HttpPost("{idProyecto:long}/script")]
    public async Task<IActionResult> GenerarGuionPorSecciones(
        long idProyecto,
        [FromQuery] OllamaMode mode = OllamaMode.Borrador  ,    
        CancellationToken ct = default)
    {
        var sw = Stopwatch.StartNew();

        var opt = BuildOptionsFromProfile(mode);
        var (path, _) = await _svc.GenerarGuionDesdeOutlineAsync(idProyecto, opt, ct);

        sw.Stop();
        var elapsedMs = sw.ElapsedMilliseconds;

        return Ok(new
        {
            ok = true,
            scriptPath = path,
            mode = opt.Mode.ToString(),
            minutos = opt.MinutosObjetivo,
            elapsedMs
        });
    }

    // ---- helpers ----

    private OllamaScriptGenRequest BuildOptionsFromProfile(OllamaMode mode)
    {
        var baseProfile = _profiles.Get(mode);

        return new OllamaScriptGenRequest
        {
            Mode = baseProfile.Mode,
            MinutosObjetivo = baseProfile.MinutosObjetivo,
            Secciones = baseProfile.Secciones,          
            PalabrasPorMinuto = baseProfile.PalabrasPorMinuto          
        };
    }
}
