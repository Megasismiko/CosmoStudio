using CosmoStudio.Model;


namespace CosmoStudio.Infraestructura.DAL.Repos.Interfaces;


public interface IRecursoRepositorio
{
    Task<List<Recurso>> ListarPorProyectoAsync(long idProyecto, CancellationToken ct);
    Task<List<Recurso>> ListarPorProyectoYTipoAsync(long idProyecto, string tipo, CancellationToken ct);
    Task AgregarAsync(Recurso recurso, CancellationToken ct);
    Task EliminarAsync(long id, CancellationToken ct);
    Task GuardarCambiosAsync(CancellationToken ct);
}
