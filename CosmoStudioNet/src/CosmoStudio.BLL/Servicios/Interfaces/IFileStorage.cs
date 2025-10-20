namespace CosmoStudio.BLL.Servicios.Interfaces
{
    public interface IFileStorage
    {
        Task WriteAllTextAsync(string path, string content, CancellationToken ct);
        Task EnsureDirectoryAsync(string dir, CancellationToken ct);
    }
}
