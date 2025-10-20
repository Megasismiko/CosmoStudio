using CosmoStudio.BLL.Kokoro;
using CosmoStudio.Common;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Net.Http.Json;


namespace CosmoStudio.Infraestructura.TTS
{

    public class KokoroClient : IkokoroClient
    {
        private readonly HttpClient _http;
        private readonly KokoroOptions _opt;

        public KokoroClient(HttpClient http, IOptions<KokoroOptions> opt)
        {
            _http = http;
            _opt = opt.Value;
        }

        public async Task<string[]> ListVoicesAsync(CancellationToken ct = default)
        {
            using var res = await _http.GetAsync(_opt.BaseUrl+ "/audio/voices", ct);
            res.EnsureSuccessStatusCode();
            var arr = await res.Content.ReadFromJsonAsync<string[]>(cancellationToken: ct) ?? Array.Empty<string>();
            return arr;
        }

        public async Task<byte[]> SynthesizeAsync(string text, CancellationToken ct = default)
        {
            var req = new KokoroSpeechRequest(
                 model: _opt.Model,
                 input: text,
                 voice: _opt.Voice,
                 response_format: _opt.Format,
                 speed: _opt.Speed
             );

            using var msg = new HttpRequestMessage(HttpMethod.Post, $"{_opt.BaseUrl}/audio/speech");
            msg.Content = JsonContent.Create(req);
            msg.Headers.Accept.Clear();
            msg.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/octet-stream"));

            using var res = await _http.SendAsync(msg, HttpCompletionOption.ResponseHeadersRead, ct);
            res.EnsureSuccessStatusCode();
            return await res.Content.ReadAsByteArrayAsync(ct);
        }
    }
}
