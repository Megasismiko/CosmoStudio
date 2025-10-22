using CosmoStudio.Model;


namespace CosmoStudio.Infraestructura.DAL.Repos.Interfaces;


public interface IRecursoRepositorio
{
    Task<List<Recurso>> ListarPorProyectoAsync(long idProyecto, CancellationToken ct);
    Task<List<Recurso>> ListarPorProyectoYTipoAsync(long idProyecto, string tipo, CancellationToken ct);
    Task<List<Recurso>> ListarPorGuionAsync(long idGuion, CancellationToken ct);
    Task<List<Recurso>> ListarPorGuionYTipoAsync(long idGuion, string tipo, CancellationToken ct);
    Task<Recurso?> GetByIdAsync(long idRecurso, CancellationToken ct);
    Task AgregarAsync(Recurso recurso, CancellationToken ct);
    Task<bool> TryEliminarAsync(long id, CancellationToken ct);
    Task GuardarCambiosAsync(CancellationToken ct);
    
}
