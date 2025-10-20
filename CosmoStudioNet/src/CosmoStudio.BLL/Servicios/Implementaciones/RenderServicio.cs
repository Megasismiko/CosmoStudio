using CosmoStudio.BLL.Servicios.Interfaces;
using CosmoStudio.Infraestructura.DAL.Repos.Interfaces;
using CosmoStudio.Model;


namespace CosmoStudio.BLL.Servicios.Implementaciones;

public class RenderServicio : IRenderServicio
{
    private readonly ITareaRenderRepositorio _repoTareas;
    private readonly IProyectoRepositorio _repoProyectos;
    private readonly ILogRepositorio _repoLogs;

    public RenderServicio(
        ITareaRenderRepositorio repoTareas,
        IProyectoRepositorio repoProyectos,
        ILogRepositorio repoLogs)
    {
        _repoTareas = repoTareas;
        _repoProyectos = repoProyectos;
        _repoLogs = repoLogs;
    }

    public async Task<TareaRender> EncolarAsync(long idProyecto, int minutos, CancellationToken ct)
    {
        var proyecto = await _repoProyectos.ObtenerPorIdAsync(idProyecto, ct)
            ?? throw new InvalidOperationException("Proyecto no encontrado");

        var t = new TareaRender
        {            
            IdProyecto = proyecto.Id,
            Estado = "EnCola",
            DuracionMinutos = minutos <= 0 ? 60 : minutos,
            FechaCreacion = DateTime.UtcNow
        };
        await _repoTareas.EncolarAsync(t, ct);
        await _repoTareas.GuardarCambiosAsync(ct);

        await _repoLogs.AgregarAsync(new Log
        {
            IdTareaRender = t.Id,
            Nivel = "Info",
            Mensaje = "Tarea encolada",
            FechaCreacion = DateTime.UtcNow
        }, ct);
        await _repoLogs.GuardarCambiosAsync(ct);

        return t;
    }

    public Task<TareaRender?> ObtenerAsync(long idTarea, CancellationToken ct) =>
        _repoTareas.ObtenerPorIdAsync(idTarea, ct);

    public Task<List<TareaRender>> ListarPorProyectoAsync(long idProyecto, CancellationToken ct) =>
        _repoTareas.ListarPorProyectoAsync(idProyecto, ct);

    public Task<List<TareaRender>> ListarPorEstadoAsync(string estado, int top, CancellationToken ct) =>
        _repoTareas.ListarPorEstadoAsync(estado, top <= 0 ? 20 : top, ct);

    public async Task MarcarEnEjecucionAsync(long idTarea, DateTime inicioUtc, CancellationToken ct)
    {
        await _repoTareas.MarcarEnEjecucionAsync(idTarea, inicioUtc, ct);
        await _repoTareas.GuardarCambiosAsync(ct);

        await _repoLogs.AgregarAsync(new Log
        {
            IdTareaRender = idTarea,
            Nivel = "Info",
            Mensaje = "Tarea en ejecución",
            FechaCreacion = DateTime.UtcNow
        }, ct);
        await _repoLogs.GuardarCambiosAsync(ct);
    }

    public async Task MarcarCompletadoAsync(long idTarea, DateTime finUtc, string? rutaVideo, string? rutasJson, CancellationToken ct)
    {
        await _repoTareas.MarcarCompletadoAsync(idTarea, finUtc, rutaVideo, rutasJson, ct);
        await _repoTareas.GuardarCambiosAsync(ct);

        await _repoLogs.AgregarAsync(new Log
        {
            IdTareaRender = idTarea,
            Nivel = "Info",
            Mensaje = "Tarea completada",
            FechaCreacion = DateTime.UtcNow
        }, ct);
        await _repoLogs.GuardarCambiosAsync(ct);
    }

    public async Task MarcarErrorAsync(long idTarea, string mensaje, CancellationToken ct)
    {
        await _repoTareas.MarcarErrorAsync(idTarea, mensaje, ct);
        await _repoTareas.GuardarCambiosAsync(ct);

        await _repoLogs.AgregarAsync(new Log
        {
            IdTareaRender = idTarea,
            Nivel = "Error",
            Mensaje = mensaje,
            FechaCreacion = DateTime.UtcNow
        }, ct);
        await _repoLogs.GuardarCambiosAsync(ct);
    }
}
