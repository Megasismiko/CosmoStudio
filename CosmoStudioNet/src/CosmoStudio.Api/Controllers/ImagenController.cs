using CosmoStudio.BLL.Servicios.Interfaces;
using CosmoStudio.Model;
using Microsoft.AspNetCore.Mvc;

namespace CosmoStudio.Api.Controllers
{
    public class ImagenController : ControllerBase
    {
        private readonly IImagenServicio _imagenes;
        private readonly IRecursoServicio _recursos;

        public ImagenController(IImagenServicio imagenes, IRecursoServicio recursos)
        {
            _imagenes = imagenes;
            _recursos = recursos;
        }

        [HttpPost("api/proyectos/{idProyecto:long}/render")]
        public async Task<IActionResult> Render(long idProyecto, CancellationToken ct)
        {
            var ids = await _imagenes.GenerarImagenesDesdeGuionPorSeccionesAsync(idProyecto, ct);
            if (ids.Count == 0)
                return NotFound("No se generaron imágenes.");

            // tomar el primer recurso y devolver el archivo real
            var recurso = await _recursos.GetByIdAsync(ids.First(), ct);
            var bytes = await System.IO.File.ReadAllBytesAsync(recurso.StoragePath, ct);
            return File(bytes, "image/png", Path.GetFileName(recurso.StoragePath));
        }

        [HttpGet("api/recursos/{id:long}/download")]
        public async Task<IActionResult> Descargar(long id, CancellationToken ct)
        {
            var recurso = await _recursos.GetByIdAsync(id, ct);
            var bytes = await System.IO.File.ReadAllBytesAsync(recurso.StoragePath, ct);
            return File(bytes, "image/png", Path.GetFileName(recurso.StoragePath));
        }
    }

}
