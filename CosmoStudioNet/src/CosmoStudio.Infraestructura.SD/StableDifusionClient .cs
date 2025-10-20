using CosmoStudio.BLL.StableDifussion;
using CosmoStudio.Common;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text.Json;

namespace CosmoStudio.Infraestructura.SD
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

        public async Task<IReadOnlyList<string>> GetSamplersAsync(CancellationToken ct = default)
        {
            using var res = await _http.GetAsync("/sdapi/v1/samplers", ct);
            res.EnsureSuccessStatusCode();
            using var s = await res.Content.ReadAsStreamAsync(ct);
            var arr = await JsonSerializer.DeserializeAsync<List<SamplerDto>>(s, _json, ct) ?? new();
            return arr.Select(x => x.Name).ToList();
        }

        public async Task<IReadOnlyList<string>> GetModelsAsync(CancellationToken ct = default)
        {
            using var res = await _http.GetAsync("/sdapi/v1/sd-models", ct);
            res.EnsureSuccessStatusCode();
            var arr = await res.Content.ReadFromJsonAsync<List<ModelDto>>(_json, ct) ?? new();
            return arr.Select(x => x.ModelName).ToList();
        }

        public async Task<IReadOnlyList<string>> GetLorasAsync(CancellationToken ct = default)
        {
            using var res = await _http.GetAsync("/sdapi/v1/loras", ct);
            res.EnsureSuccessStatusCode();
            var arr = await res.Content.ReadFromJsonAsync<List<LoraDto>>(_json, ct) ?? new();
            return arr.Select(x => x.Name).ToList();
        }

        public async Task<Txt2ImgResponse> Txt2ImgAsync(Txt2ImgRequest req, CancellationToken ct = default)
        {
            // completa con defaults de Options si el caller no los setea
            req.Steps = req.Steps == 0 ? _opt.Steps : req.Steps;
            req.CfgScale = req.CfgScale == 0 ? _opt.CfgScale : req.CfgScale;
            req.Width = req.Width == 0 ? _opt.Width : req.Width;
            req.Height = req.Height == 0 ? _opt.Height : req.Height;
            req.SamplerName = string.IsNullOrWhiteSpace(req.SamplerName) ? _opt.DefaultSampler : req.SamplerName;
            if (req.EnableHr && (req.HrScale == 0)) req.HrScale = _opt.HiresUpscale;
            if (req.EnableHr && string.IsNullOrWhiteSpace(req.HrUpscaler)) req.HrUpscaler = _opt.Upscaler;

            using var res = await _http.PostAsJsonAsync("/sdapi/v1/txt2img", req, _json, ct);
            res.EnsureSuccessStatusCode();
            var body = await res.Content.ReadFromJsonAsync<Txt2ImgResponse>(_json, ct);
            return body ?? new Txt2ImgResponse();
        }

        public async Task SetOptionsAsync(SetOptionsRequest req, CancellationToken ct = default)
        {
            using var res = await _http.PostAsJsonAsync("/sdapi/v1/options", req, _json, ct);
            res.EnsureSuccessStatusCode();
        }

        // DTOs internos para deserialización ligera
        private sealed record SamplerDto(string Name);
        private sealed record ModelDto(string Title, string ModelName);
        private sealed record LoraDto(string Name);
    }

}