namespace CosmoStudio.BLL.Kokoro
{
    public interface IkokoroClient
    {
        Task<byte[]> SynthesizeAsync(string text, CancellationToken ct );
    }
}
