using CosmoStudio.BLL.Servicios.Interfaces;
using CosmoStudio.Common;
using CosmoStudio.Common.Interfaces;
using CosmoStudio.Common.Requests;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace CosmoStudio.API.Controllers
{
    public sealed class PipelineRunRequest
    {
        // Opciones mínimas para la generación (si no las envían, aplicamos defaults sensatos)
        public OllamaMode Mode { get; set; } = OllamaMode.Borrador; // o Produccion
        public int MinutosObjetivo { get; set; } = 6;              // duración total deseada
        public int Secciones { get; set; } = 8;                    // nº mínimo de secciones
        public int PalabrasPorMinuto { get; set; } = 130;          // WPM para dimensionar secciones
    }

    [ApiController]
    [Route("api/pipeline")]
    public class PipelineController : ControllerBase
    {
        private readonly IGuionServicio _guiones;
        private readonly IAudioServicio _audios;
        private readonly IImagenServicio _imagenes;
        private readonly IRenderServicio _render;
        private readonly IScriptGenOptionsProvider _profiles;

        public PipelineController(
            IGuionServicio guiones,
            IAudioServicio audios,
            IImagenServicio imagenes,
            IRenderServicio render,
            IScriptGenOptionsProvider profiles)
        {
            _guiones = guiones;
            _audios = audios;
            _imagenes = imagenes;
            _render = render;
            _profiles = profiles;
        }
        private OllamaScriptGenRequest BuildOptionsFromProfile(OllamaMode mode)
        {
            var baseProfile = _profiles.Get(mode);

            return new OllamaScriptGenRequest
            {
                Mode = baseProfile.Mode,
                MinutosObjetivo = baseProfile.MinutosObjetivo,
                Secciones = baseProfile.Secciones,                
                PalabrasPorMinuto = baseProfile.PalabrasPorMinuto,      
            };
        }
        /// <summary>
        /// Ejecuta TODO el pipeline: outline → guion → audios → imágenes → manifest → render.
        /// </summary>
        [HttpPost("run")]
        public async Task<IActionResult> Run(long idProyecto, OllamaMode modo, CancellationToken ct)
        {
            var swGlobal = Stopwatch.StartNew();

            // Defaults de opciones de guion
            var opt = BuildOptionsFromProfile(modo);


            var steps = new List<object>();

            try
            {
                // 1) OUTLINE
                var swOutline = Stopwatch.StartNew();
                var (outlinePath, outlineText) = await _guiones.GenerarOutlineAsync(idProyecto, opt, ct);
                swOutline.Stop();
                steps.Add(new
                {
                    step = "outline",
                    path = outlinePath,
                    length = outlineText?.Length ?? 0,
                    elapsedMs = swOutline.ElapsedMilliseconds
                });

                // 2) GUION (por secciones) desde outline
                var swScript = Stopwatch.StartNew();
                var (scriptPath, scriptText) = await _guiones.GenerarGuionDesdeOutlineAsync(idProyecto, opt, ct);
                swScript.Stop();
                steps.Add(new
                {
                    step = "script",
                    path = scriptPath,
                    length = scriptText?.Length ?? 0,
                    elapsedMs = swScript.ElapsedMilliseconds
                });

                // 3) AUDIOS por sección
                var swAudio = Stopwatch.StartNew();
                var audioIds = await _audios.GenerarVozDesdeGuionPorSeccionesAsync(idProyecto, ct);
                swAudio.Stop();
                steps.Add(new
                {
                    step = "audio",
                    created = audioIds.Count,
                    ids = audioIds,
                    elapsedMs = swAudio.ElapsedMilliseconds
                });

                // 4) IMÁGENES por sección
                var swImg = Stopwatch.StartNew();
                var imgIds = await _imagenes.GenerarImagenesDesdeGuionPorSeccionesAsync(idProyecto, ct);
                swImg.Stop();
                steps.Add(new
                {
                    step = "image",
                    created = imgIds.Count,
                    ids = imgIds,
                    elapsedMs = swImg.ElapsedMilliseconds
                });

                // 5) MANIFEST
                var swManifest = Stopwatch.StartNew();
                var (manifestPath, manifest) = await _render.GenerarManifestAsync(idProyecto, ct: ct);
                swManifest.Stop();
                steps.Add(new
                {
                    step = "manifest",
                    path = manifestPath,
                    sections = manifest.Sections?.Count ?? 0,
                    elapsedMs = swManifest.ElapsedMilliseconds
                });

                // 6) RENDER
                var swRender = Stopwatch.StartNew();
                var finalPath = await _render.RenderDesdeManifestAsync(manifestPath, ct);
                swRender.Stop();
                steps.Add(new
                {
                    step = "render",
                    path = finalPath,
                    elapsedMs = swRender.ElapsedMilliseconds
                });

                swGlobal.Stop();
                return Ok(new
                {
                    projectId = idProyecto,
                    options = opt,
                    steps,
                    final = new { manifestPath, videoPath = finalPath },
                    elapsedMs = swGlobal.ElapsedMilliseconds
                });
            }
            catch (Exception ex)
            {
                swGlobal.Stop();
                return BadRequest(new
                {
                    success = false,
                    projectId = idProyecto,
                    error = ex.Message,
                    elapsedMs = swGlobal.ElapsedMilliseconds,
                    steps
                });
            }
        }
    }
}
