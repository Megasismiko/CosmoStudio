using CosmoStudio.Model;

namespace CosmoStudio.BLL.Servicios.Interfaces;

public interface IRenderServicio
{
    Task<TareaRender> EncolarAsync(long idProyecto, int minutos, CancellationToken ct);
    Task<TareaRender?> ObtenerAsync(long idTarea, CancellationToken ct);
    Task<List<TareaRender>> ListarPorProyectoAsync(long idProyecto, CancellationToken ct);
    Task<List<TareaRender>> ListarPorEstadoAsync(string estado, int top, CancellationToken ct);

    Task MarcarEnEjecucionAsync(long idTarea, DateTime inicioUtc, CancellationToken ct);
    Task MarcarCompletadoAsync(long idTarea, DateTime finUtc, string? rutaVideo, string? rutasJson, CancellationToken ct);
    Task MarcarErrorAsync(long idTarea, string mensaje, CancellationToken ct);
}
