using Microsoft.EntityFrameworkCore;

namespace CosmoStudio.Infraestructura.DAL.Repos.Implementaciones;

using CosmoStudio.Infraestructura.DAL.Scaffolding;
using CosmoStudio.Model;
using Interfaces;

public class TareaRenderRepositorio : ITareaRenderRepositorio
{
    private readonly CosmoDbContext _db;
    public TareaRenderRepositorio(CosmoDbContext db) => _db = db;

    public Task<TareaRender?> ObtenerPorIdAsync(long id, CancellationToken ct) =>
        _db.TareasRenders.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id, ct);

    public Task<List<TareaRender>> ListarPorProyectoAsync(long idProyecto, CancellationToken ct) =>
        _db.TareasRenders.AsNoTracking()
           .Where(r => r.IdProyecto == idProyecto)
           .OrderByDescending(r => r.FechaCreacion)
           .ToListAsync(ct);

    public Task<List<TareaRender>> ListarPorEstadoAsync(string estado, int top, CancellationToken ct) =>
        _db.TareasRenders.AsNoTracking()
           .Where(r => r.Estado == estado)
           .OrderBy(r => r.FechaCreacion)
           .Take(top)
           .ToListAsync(ct);

    public async Task EncolarAsync(TareaRender tarea, CancellationToken ct)
    {
        await _db.TareasRenders.AddAsync(tarea, ct);
    }

    public async Task MarcarEnEjecucionAsync(long id, DateTime inicioUtc, CancellationToken ct)
    {
        var t = await _db.TareasRenders.FirstAsync(x => x.Id == id, ct);
        t.Estado = "EnEjecucion";
        t.FechaInicio = inicioUtc;
    }

    public async Task MarcarCompletadoAsync(long id, DateTime finUtc, string? rutaVideo, string? rutasJson, CancellationToken ct)
    {
        var t = await _db.TareasRenders.FirstAsync(x => x.Id == id, ct);
        t.Estado = "Completado";
        t.FechaFin = finUtc;
        t.RutaVideoSalida = rutaVideo;
        t.RutasSalidaJson = rutasJson;
    }

    public async Task MarcarErrorAsync(long id, string mensaje, CancellationToken ct)
    {
        var t = await _db.TareasRenders.FirstAsync(x => x.Id == id, ct);
        t.Estado = "Error";        
    }

    public Task GuardarCambiosAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);
}
