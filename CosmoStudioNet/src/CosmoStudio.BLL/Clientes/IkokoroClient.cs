namespace CosmoStudio.BLL.Clientes
{
    public interface IkokoroClient
    {
        Task<byte[]> SynthesizeAsync(string text, CancellationToken ct );
    }
}
