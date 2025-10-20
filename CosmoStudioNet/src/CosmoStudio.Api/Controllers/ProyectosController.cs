using CosmoStudio.BLL.Servicios.Interfaces;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/proyectos")]
public class ProyectosController : ControllerBase
{
    private readonly IProyectoServicio _svc;
    public ProyectosController(IProyectoServicio svc) => _svc = svc;

    public record CrearProyectoDto(string titulo, string tema, string origen = "Manual");

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] CrearProyectoDto dto, CancellationToken ct)
        => Ok(await _svc.CrearAsync(dto.titulo, dto.tema, dto.origen, ct));

    [HttpGet("ultimos")]
    public async Task<IActionResult> Ultimos([FromQuery] int top = 5, CancellationToken ct = default)
        => Ok(await _svc.ListarUltimosAsync(top, ct));
}

