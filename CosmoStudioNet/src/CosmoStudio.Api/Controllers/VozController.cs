using CosmoStudio.BLL.Servicios.Interfaces;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/voz")]
public class VozController : ControllerBase
{
    private readonly IAudioServicio _voz;

    public VozController(IAudioServicio voz) => _voz = voz;

    [HttpPost("{idProyecto:long}")]
    public async Task<IActionResult> GenerarAudio(long idProyecto, CancellationToken ct)
    {
        var idRecurso = await _voz.GenerarVozDesdeGuionAsync(idProyecto, ct);
        return Ok(new { RecursoId = idRecurso });
    }
}
