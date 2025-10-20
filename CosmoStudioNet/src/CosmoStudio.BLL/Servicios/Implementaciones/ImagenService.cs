using CosmoStudio.BLL.Servicios.Interfaces;
using CosmoStudio.BLL.StableDifussion;
using CosmoStudio.Common;

namespace CosmoStudio.BLL.Servicios.Implementaciones
{
    public sealed class ImagenService : IImagenService
    {
        private readonly IStableDifusionClient _stableClient;

        public ImagenService(IStableDifusionClient stableClient)
        {
            _stableClient = stableClient;
        }

        public async Task<byte[]> GenerarImagenAsync(
            string prompt,
            string? negativePrompt = null,
            int width = 1920,
            int height = 1080,
            double cfgScale = 8.0,
            long seed = -1,
            CancellationToken ct = default)
        {
            var req = new Txt2ImgRequest
            {
                Prompt = prompt,
                NegativePrompt = negativePrompt ?? "text, watermark, blurry, lowres, artifacts",
                Steps = 35,
                CfgScale = cfgScale,
                Width = width,
                Height = height,
                SamplerName = "DPM++ 2M",
                EnableHr = true,
                HrScale = 1.8,
                HrUpscaler = "Latent (antialiased)",
                Seed = seed
            };

            var res = await _stableClient.Txt2ImgAsync(req, ct);
            if (res.Images is null || res.Images.Count == 0)
                throw new InvalidOperationException("No se recibió ninguna imagen desde Stable Diffusion.");

            return Convert.FromBase64String(res.Images.First());
        }

        public async Task<byte[]> GenerarImagenAvanzadaAsync(Txt2ImgRequest request, CancellationToken ct = default)
        {
            var res = await _stableClient.Txt2ImgAsync(request, ct);
            if (res.Images is null || res.Images.Count == 0)
                throw new InvalidOperationException("No se recibió ninguna imagen desde Stable Diffusion.");

            return Convert.FromBase64String(res.Images.First());
        }
    }

}
