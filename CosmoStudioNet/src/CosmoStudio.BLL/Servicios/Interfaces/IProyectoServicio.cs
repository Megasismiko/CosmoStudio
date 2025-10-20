using CosmoStudio.Model;


namespace CosmoStudio.BLL.Servicios.Interfaces;

public interface IProyectoServicio
{
    Task<Proyecto> CrearAsync(string titulo, string tema, string origen, CancellationToken ct);
    Task<Proyecto?> ObtenerAsync(long id, CancellationToken ct);
    Task<List<Proyecto>> ListarUltimosAsync(int top, CancellationToken ct);
    Task<List<Proyecto>> BuscarAsync(string texto, int top, CancellationToken ct);
    Task<bool> EliminarAsync(long id, CancellationToken ct);
}
