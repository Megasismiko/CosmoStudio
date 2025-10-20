using CosmoStudio.Model;


namespace CosmoStudio.Infraestructura.DAL.Repos.Interfaces;

public interface ILogRepositorio
{
    Task AgregarAsync(Log log, CancellationToken ct);
    Task<List<Log>> ListarPorTareaAsync(long idTarea, int top, CancellationToken ct);

    Task GuardarCambiosAsync(CancellationToken ct);
}
