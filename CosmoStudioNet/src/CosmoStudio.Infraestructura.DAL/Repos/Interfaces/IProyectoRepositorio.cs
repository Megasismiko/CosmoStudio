using CosmoStudio.Model;


namespace CosmoStudio.Infraestructura.DAL.Repos.Interfaces;

public interface IProyectoRepositorio
{
    Task<Proyecto?> ObtenerPorIdAsync(long id, CancellationToken ct);
    Task<List<Proyecto>> ListarUltimosAsync(int top, CancellationToken ct);
    Task<List<Proyecto>> BuscarPorTemaAsync(string texto, int top, CancellationToken ct);

    Task CrearAsync(Proyecto proyecto, CancellationToken ct);
    Task ActualizarAsync(Proyecto proyecto, CancellationToken ct);
    Task EliminarAsync(long id, CancellationToken ct);

    Task GuardarCambiosAsync(CancellationToken ct);
}
