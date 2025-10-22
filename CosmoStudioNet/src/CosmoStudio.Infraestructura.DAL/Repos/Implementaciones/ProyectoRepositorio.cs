using CosmoStudio.Infraestructura.DAL.Repos.Interfaces;
using CosmoStudio.Infraestructura.DAL.Scaffolding;
using CosmoStudio.Model;
using Microsoft.EntityFrameworkCore;

namespace CosmoStudio.Infraestructura.DAL.Repos.Implementaciones;

public class ProyectoRepositorio : IProyectoRepositorio
{
    private readonly CosmoDbContext _db;

    public ProyectoRepositorio(CosmoDbContext db) => _db = db;

    public Task<Proyecto?> ObtenerPorIdAsync(long id, CancellationToken ct) =>
        _db.Proyectos
            .Include(p => p.Guion)
                .ThenInclude(g => g.CurrentVersion)
                .ThenInclude(v => v.ScriptRecurso)
            .Include(p => p.Guion)
                .ThenInclude(g => g.CurrentVersion)
                .ThenInclude(v => v.OutlineRecurso)
            .Include(p => p.TareasRender)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, ct);
    public Task<List<Proyecto>> ListarUltimosAsync(int top, CancellationToken ct) =>
        _db.Proyectos.AsNoTracking()
           .OrderByDescending(p => p.FechaCreacion)
           .Take(top)
           .ToListAsync(ct);

    public Task<List<Proyecto>> BuscarPorTemaAsync(string texto, int top, CancellationToken ct)
    {
        texto = texto?.Trim() ?? string.Empty;
        return _db.Proyectos.AsNoTracking()
            .Where(p => p.Tema.Contains(texto) || p.Titulo.Contains(texto))
            .OrderByDescending(p => p.FechaCreacion)
            .Take(top)
            .ToListAsync(ct);
    }

    public async Task CrearAsync(Proyecto proyecto, CancellationToken ct)
    {
        await _db.Proyectos.AddAsync(proyecto, ct);
    }

    public Task ActualizarAsync(Proyecto proyecto, CancellationToken ct)
    {
        _db.Proyectos.Update(proyecto);
        return Task.CompletedTask;
    }

    public async Task EliminarAsync(long id, CancellationToken ct)
    {
        var entity = await _db.Proyectos.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (entity is not null)
            _db.Proyectos.Remove(entity);
    }

    public Task GuardarCambiosAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);
}
