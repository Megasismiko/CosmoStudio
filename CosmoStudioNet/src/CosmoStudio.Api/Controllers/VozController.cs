using CosmoStudio.BLL.Servicios.Interfaces;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/voz")]
public class VozController : ControllerBase
{
    private readonly IVozServicio _voz;

    public VozController(IVozServicio voz) => _voz = voz;

    [HttpPost("{idProyecto:long}")]
    public async Task<IActionResult> GenerarAudio(long idProyecto, CancellationToken ct)
    {
        var idRecurso = await _voz.GenerarAudioProyectoAsync(idProyecto, ct);
        return Ok(new { RecursoId = idRecurso });
    }
}
