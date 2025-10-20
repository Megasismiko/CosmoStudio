using CosmoStudio.BLL.Servicios.Interfaces;
using CosmoStudio.Common;
using Microsoft.Extensions.Options;

namespace CosmoStudio.Infraestructura.Files;

public class LocalFileStorage : IFileStorage
{
    private readonly StorageOptions _opt;
    public LocalFileStorage(IOptions<StorageOptions> opt) => _opt = opt.Value;

    public Task EnsureDirectoryAsync(string dir, CancellationToken ct)
    {
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
        return Task.CompletedTask;
    }

    public async Task WriteAllTextAsync(string path, string content, CancellationToken ct)
    {
        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(dir)) Directory.CreateDirectory(dir!);
        await File.WriteAllTextAsync(path, content, ct);
    }
}
