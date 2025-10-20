using CosmoStudio.Model;


namespace CosmoStudio.Infraestructura.DAL.Repos.Interfaces;

public interface ITareaRenderRepositorio
{
    Task<TareaRender?> ObtenerPorIdAsync(long id, CancellationToken ct);
    Task<List<TareaRender>> ListarPorProyectoAsync(long idProyecto, CancellationToken ct);
    Task<List<TareaRender>> ListarPorEstadoAsync(string estado, int top, CancellationToken ct);

    Task EncolarAsync(TareaRender tarea, CancellationToken ct);
    Task MarcarEnEjecucionAsync(long id, DateTime inicioUtc, CancellationToken ct);
    Task MarcarCompletadoAsync(long id, DateTime finUtc, string? rutaVideo, string? rutasJson, CancellationToken ct);
    Task MarcarErrorAsync(long id, string mensaje, CancellationToken ct);

    Task GuardarCambiosAsync(CancellationToken ct);
}
