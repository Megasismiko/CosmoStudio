using CosmoStudio.Model;

namespace CosmoStudio.Infraestructura.DAL.Repos.Interfaces
{
    public interface IGuionVersionRepositorio
    {
        Task<GuionVersion?> ObtenerAsync(long idVersion, CancellationToken ct);
        Task<List<GuionVersion>> ListarPorGuionAsync(long idGuion, CancellationToken ct);
        Task<GuionVersion> CrearAsync(long idGuion, int numeroVersion, long? outlineRecursoId, long? scriptRecursoId, string? notas, CancellationToken ct);
        Task EstablecerComoVigenteAsync(long idGuion, long idVersion, CancellationToken ct);
        Task SetScriptAsync(long idVersion, long idRecursoScript, CancellationToken ct);
        Task GuardarCambiosAsync(CancellationToken ct);
    }
}
