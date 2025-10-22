using CosmoStudio.Model;

namespace CosmoStudio.Infraestructura.DAL.Repos.Interfaces
{
    public interface IGuionAudioRepositorio
    {
        Task<List<GuionAudio>> ListarPorVersionAsync(long idGuionVersion, CancellationToken ct);
        Task<int> ObtenerSiguienteOrdenAsync(long idGuionVersion, CancellationToken ct);
        Task AgregarAsync(GuionAudio audio, CancellationToken ct);
        Task ReordenarAsync(long idGuionVersion, IEnumerable<(long id, int nuevoOrden)> ordenes, CancellationToken ct);
        Task EliminarAsync(long id, CancellationToken ct);
        Task GuardarCambiosAsync(CancellationToken ct);
    }
}
