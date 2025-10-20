using CosmoStudio.Model;


namespace CosmoStudio.BLL.Servicios.Interfaces;

public interface IRecursoServicio
{
    Task<List<Recurso>> ListarAsync(long idProyecto, string? tipo, CancellationToken ct);
    Task<Recurso> AgregarAsync(long idProyecto, string tipo, string ruta, string? metaJson, CancellationToken ct);
    Task<bool> EliminarAsync(long idRecurso, CancellationToken ct);
}
