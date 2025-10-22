using CosmoStudio.BLL.Servicios.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CosmoStudio.API.Controllers
{
    [ApiController]
    [Route("api/video")]
    public class VideoController : ControllerBase
    {
        private readonly IRenderServicio _render;

        public VideoController(IRenderServicio render)
        {
            _render = render;
        }

        /// <summary>
        /// Genera el manifest (timeline) desde los recursos del proyecto.
        /// </summary>
        [HttpPost("manifest")]
        public async Task<IActionResult> GenerarManifest(long idProyecto, CancellationToken ct)
        {
            try
            {
                var (manifestPath, manifest) = await _render.GenerarManifestAsync(idProyecto, ct: ct);
                return Ok(new { success = true, path = manifestPath, manifest });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, error = ex.Message });
            }
        }

        /// <summary>
        /// Renderiza el vídeo final del proyecto (genera/actualiza el manifest y renderiza).
        /// </summary>
        [HttpPost("render")]
        public async Task<IActionResult> Render(long idProyecto, CancellationToken ct)
        {
            try
            {
                var finalPath = await _render.RenderProyectoVigenteAsync(idProyecto, ct);
                return Ok(new { success = true, finalPath });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, error = ex.Message });

            }


        }
    }
}
