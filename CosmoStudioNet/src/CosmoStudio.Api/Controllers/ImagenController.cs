using CosmoStudio.BLL.Servicios.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CosmoStudio.Api.Controllers
{
    public class ImagenController : ControllerBase
    {
        private readonly IImagenService _imagenes;

        public ImagenController(IImagenService imagenes)
        {
            _imagenes = imagenes;
        }

        [HttpPost("api/render")]
        public async Task<IActionResult> Render([FromQuery] string promt , CancellationToken ct)
        {           
            promt = string.IsNullOrEmpty(promt) ?  "<lora:Galaxy_SDXL:0.3> a vast spiral galaxy in deep space, ultra realistic, HDR" : promt;
            var bytes = await _imagenes.GenerarImagenAsync(promt, ct: ct);
            return File(bytes, "image/png");
        }
    }

}
