using CosmoStudio.Common;

namespace CosmoStudio.BLL.Servicios.Interfaces
{
    public interface ILocalFileStorage
    {
        Task<string> SaveBytesAsync(
            long idProyecto,
            int version,
            TipoRecurso tipo,
            ReadOnlyMemory<byte> data,
            string extension,
            int? index = null,
            CancellationToken ct = default);

        Task<string> SaveTextAsync(
            long idProyecto,
            int version,
            TipoRecurso tipo,
            string content,
            string extension,
            int? index = null,
            CancellationToken ct = default);

        string GetResourceDirectory(long idProyecto, int version, TipoRecurso tipo);
        string GetFilePath(long idProyecto, int version, TipoRecurso tipo, string fileName);
    }
}