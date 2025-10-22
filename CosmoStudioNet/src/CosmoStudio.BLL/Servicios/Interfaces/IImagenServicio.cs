using CosmoStudio.Common;

namespace CosmoStudio.BLL.Servicios.Interfaces
{
    public interface IImagenServicio
    {
        Task<IReadOnlyList<long>> GenerarImagenesDesdeGuionPorSeccionesAsync(long idProyecto, CancellationToken ct = default);
    }
}
