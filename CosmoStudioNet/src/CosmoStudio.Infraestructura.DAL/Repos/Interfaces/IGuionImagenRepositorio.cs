using CosmoStudio.Model;

namespace CosmoStudio.Infraestructura.DAL.Repos.Interfaces
{
    public interface IGuionImagenRepositorio
    {
        Task<List<GuionImagen>> ListarPorVersionAsync(long idGuionVersion, CancellationToken ct);
        Task<int> ObtenerSiguienteOrdenAsync(long idGuionVersion, CancellationToken ct);
        Task AgregarAsync(GuionImagen imagen, CancellationToken ct);
        Task ReordenarAsync(long idGuionVersion, IEnumerable<(long id, int nuevoOrden)> ordenes, CancellationToken ct);
        Task EliminarAsync(long id, CancellationToken ct);
        Task GuardarCambiosAsync(CancellationToken ct);
    }
}
