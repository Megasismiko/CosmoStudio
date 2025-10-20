using CosmoStudio.Common;

namespace CosmoStudio.BLL.StableDifussion
{
    public interface IStableDifusionClient
    {
        Task<IReadOnlyList<string>> GetSamplersAsync(CancellationToken ct = default);
        Task<IReadOnlyList<string>> GetModelsAsync(CancellationToken ct = default);
        Task<IReadOnlyList<string>> GetLorasAsync(CancellationToken ct = default);
        Task<Txt2ImgResponse> Txt2ImgAsync(Txt2ImgRequest req, CancellationToken ct = default);
        Task SetOptionsAsync(SetOptionsRequest req, CancellationToken ct = default);
    }
}
