using CosmoStudio.BLL.Clientes;
using CosmoStudio.Common;
using CosmoStudio.Common.Opciones;
using CosmoStudio.Common.Requests;
using CosmoStudio.Common.Responses;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text.Json;

namespace CosmoStudio.Infraestructura.T2I
{
    public sealed class StableDifusionClient : IStableDifusionClient
    {
        private readonly HttpClient _http;
        private readonly StableDiffusionOptions _opt;
        private static readonly JsonSerializerOptions _json = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public StableDifusionClient(HttpClient http, IOptions<StableDiffusionOptions> opt)
        {
            _http = http;
            _opt = opt.Value;
            if (_http.BaseAddress is null)
                _http.BaseAddress = new Uri(_opt.BaseUrl);
        }

        public async Task<Txt2ImgResponse> Txt2ImgAsync(Txt2ImgRequest req, CancellationToken ct = default)
        {
            // Completa defaults desde _opt
            req.Steps = req.Steps == 0 ? _opt.Steps : req.Steps;
            req.CfgScale = req.CfgScale == 0 ? _opt.CfgScale : req.CfgScale;
            req.Width = req.Width == 0 ? _opt.Width : req.Width;
            req.Height = req.Height == 0 ? _opt.Height : req.Height;
            req.SamplerName = string.IsNullOrWhiteSpace(req.SamplerName) ? _opt.DefaultSampler : req.SamplerName;            
            if (req.EnableHr)
            {
                if (req.HrScale == 0) req.HrScale = _opt.HiresUpscale;
                if (string.IsNullOrWhiteSpace(req.HrUpscaler)) req.HrUpscaler = _opt.Upscaler;
                if (req.DenoisingStrength is null) req.DenoisingStrength = 0.45; // valor típico útil para HIRES
            }

            using var res = await _http.PostAsJsonAsync("/sdapi/v1/txt2img", req, _json, ct);
            res.EnsureSuccessStatusCode();
            var body = await res.Content.ReadFromJsonAsync<Txt2ImgResponse>(_json, ct);
            return body ?? new Txt2ImgResponse();
        }



    }

}