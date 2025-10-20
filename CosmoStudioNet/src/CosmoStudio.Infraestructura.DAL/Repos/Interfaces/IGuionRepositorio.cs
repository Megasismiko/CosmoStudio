using CosmoStudio.Model;


namespace CosmoStudio.Infraestructura.DAL.Repos.Interfaces;

public interface IGuionRepositorio
{
    Task<Guion?> ObtenerPorProyectoAsync(long idProyecto, CancellationToken ct);
    Task CrearAsync(Guion guion, CancellationToken ct);
    Task ActualizarAsync(Guion guion, CancellationToken ct);
    Task EliminarPorProyectoAsync(long idProyecto, CancellationToken ct);

    Task GuardarCambiosAsync(CancellationToken ct);
}
