using CosmoStudio.Common;

namespace CosmoStudio.BLL.Servicios.Interfaces
{
    public interface IImagenService
    {
        /// <summary>
        /// Genera una imagen a partir de un prompt usando Stable Diffusion.
        /// </summary>
        /// <param name="prompt">Texto descriptivo para generar la imagen.</param>
        /// <param name="negativePrompt">Prompt negativo (opcional).</param>
        /// <param name="width">Ancho en píxeles.</param>
        /// <param name="height">Alto en píxeles.</param>
        /// <param name="cfgScale">Nivel de adherencia al prompt.</param>
        /// <param name="seed">Semilla para reproducibilidad (-1 = aleatoria).</param>
        /// <param name="ct">Token de cancelación.</param>
        /// <returns>Imagen generada en formato byte[] (PNG por defecto).</returns>
        Task<byte[]> GenerarImagenAsync(
            long idProyecto,
            string prompt,
            string? negativePrompt = null,
            int width = 1920,
            int height = 1080,
            double cfgScale = 8.0,
            long seed = -1,
            CancellationToken ct = default
        );

        /// <summary>
        /// Genera una imagen con configuración avanzada (pasos, sampler, hires, etc.).
        /// </summary>
        Task<byte[]> GenerarImagenAvanzadaAsync(long idProyecto, Txt2ImgRequest request, CancellationToken ct = default);
    }
}
