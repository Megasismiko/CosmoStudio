using CosmoStudio.BLL.Ollama;
using CosmoStudio.BLL.Servicios.Interfaces;
using CosmoStudio.Common;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/guion")]
public class GuionController : ControllerBase
{
    private readonly IGuionServicio _svc;
    public GuionController(IGuionServicio svc) => _svc = svc;

    [HttpPost("{idProyecto:long}/outline")]
    public async Task<IActionResult> GenerarOutline(
        long idProyecto, 
        [FromQuery] string mode = "borrador",
        [FromQuery] int minutos = 60, 
        [FromQuery] string? modelo = null, 
        CancellationToken ct = default
    )
    {
        var opt = BuildOptions(mode, minutos, modelo);
        var (path, _) = await _svc.GenerarOutlineAsync(idProyecto, opt, ct);
        return Ok(new { ok = true, outlinePath = path, mode = opt.Mode.ToString() });
    }

    [HttpPost("{idProyecto:long}/script")]
    public async Task<IActionResult> GenerarGuion(
        long idProyecto, 
        [FromQuery] string mode = "borrador",
        [FromQuery] int minutos = 60, 
        [FromQuery] string? modelo = null, 
        CancellationToken ct = default
    )
    {
        var opt = BuildOptions(mode, minutos, modelo);
        var (path, _) = await _svc.GenerarGuionDesdeOutlineAsync(idProyecto, opt, ct);
        return Ok(new { ok = true, scriptPath = path, mode = opt.Mode.ToString() });
    }

    [HttpPost("{idProyecto:long}/script-by-sections")]
    public async Task<IActionResult> GenerarGuionPorSecciones(
        long idProyecto,
        [FromQuery] string mode = "produccion",
        [FromQuery] int minutos = 60,
        [FromQuery] string? modelo = null,
        CancellationToken ct = default
    )
    {
        var opt = BuildOptions(mode, minutos, modelo); // el mismo helper que ya tienes
        var (path, _) = await _svc.GenerarGuionDesdeOutlinePorSeccionesAsync(idProyecto, opt, ct);
        return Ok(new { ok = true, scriptPath = path, mode = opt.Mode.ToString() });
    }

    private static ScriptGenOptions BuildOptions(string mode, int minutos, string? modelo)
    {
        var m = (mode ?? "borrador").Trim().ToLowerInvariant();
        if (m is "prod" or "produccion")
        {
            return new ScriptGenOptions
            {
                Mode = ScriptMode.Produccion,
                MinutosObjetivo = minutos,
                Secciones = 50,
                ParrafosPorSeccion = 2,
                PalabrasPorMinuto = 120,
                Modelo = modelo,
                Estilo = "tono calmado, estilo documental, para dormir"
            };
        }
        return new ScriptGenOptions
        {
            Mode = ScriptMode.Borrador,
            MinutosObjetivo = minutos,
            Secciones = 10,
            ParrafosPorSeccion = 1,
            PalabrasPorMinuto = 120,
            Modelo = modelo,
            Estilo = "tono calmado, resumen conciso"
        };
    }

}
