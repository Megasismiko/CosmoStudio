using CosmoStudio.BLL.Servicios.Interfaces;
using CosmoStudio.BLL.StableDifussion;
using CosmoStudio.Common;
using CosmoStudio.Model;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace CosmoStudio.BLL.Servicios.Implementaciones;

public sealed class ImagenService : IImagenService
{
    private readonly IStableDifusionClient _stableClient;
    private readonly IRecursoServicio _recursos;
    private readonly StorageOptions _storageOptions;

    public ImagenService(
        IStableDifusionClient stableClient,
        IRecursoServicio recursos,
        IOptions<StorageOptions> storageOptions)
    {
        _stableClient = stableClient;
        _recursos = recursos;
        _storageOptions = storageOptions.Value;
    }

    public async Task<byte[]> GenerarImagenAsync(
        long idProyecto,
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

        var bytes = Convert.FromBase64String(res.Images.First());
        var meta = JsonSerializer.Serialize(new
        {
            prompt,
            negativePrompt = negativePrompt ?? string.Empty,
            width,
            height,
            cfgScale,
            seed
        });

        await GuardarRecursoAsync(idProyecto, bytes, meta, ct);
        return bytes;
    }

    public async Task<byte[]> GenerarImagenAvanzadaAsync(long idProyecto, Txt2ImgRequest request, CancellationToken ct = default)
    {
        var res = await _stableClient.Txt2ImgAsync(request, ct);
        if (res.Images is null || res.Images.Count == 0)
            throw new InvalidOperationException("No se recibió ninguna imagen desde Stable Diffusion.");

        var bytes = Convert.FromBase64String(res.Images.First());
        var meta = JsonSerializer.Serialize(new
        {
            request.Prompt,
            request.NegativePrompt,
            request.Steps,
            request.CfgScale,
            request.Width,
            request.Height,
            request.SamplerName,
            request.Seed
        });

        await GuardarRecursoAsync(idProyecto, bytes, meta, ct);
        return bytes;
    }

    private async Task GuardarRecursoAsync(long idProyecto, byte[] bytes, string metaJson, CancellationToken ct)
    {
        if (idProyecto <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(idProyecto), "El identificador del proyecto debe ser mayor a cero.");
        }

        var runDir = Path.Combine(_storageOptions.RunsRoot, idProyecto.ToString());
        Directory.CreateDirectory(runDir);

        var fileName = $"img_{DateTime.UtcNow:yyyyMMdd_HHmmssfff}.png";
        var ruta = Path.Combine(runDir, fileName);

        await File.WriteAllBytesAsync(ruta, bytes, ct);
        await _recursos.AgregarAsync(idProyecto, TipoRecurso.Imagen.ToString(), ruta, metaJson, ct);
    }
}
