using CosmoStudio.Infraestructura.DAL.Scaffolding;
using CosmoStudio.Infraestructura.DAL.Repos.Interfaces;
using CosmoStudio.Model;
using Microsoft.EntityFrameworkCore;

namespace CosmoStudio.Infraestructura.DAL.Repos.Implementaciones;

public class RecursoRepositorio : IRecursoRepositorio
{
    private readonly CosmoDbContext _db;

    public RecursoRepositorio(CosmoDbContext db)
    {
        _db = db;
    }

    public Task<Recurso?> GetByIdAsync(long idRecurso, CancellationToken ct) =>
         _db.Recursos
            .Where(r => r.Id == idRecurso)
            .AsNoTracking()
            .FirstOrDefaultAsync();
   

    public Task<List<Recurso>> ListarPorProyectoAsync(long idProyecto, CancellationToken ct) =>
        _db.Recursos
            .Where(r => r.IdProyecto == idProyecto)
            .OrderByDescending(r => r.FechaCreacion)
            .AsNoTracking()
            .ToListAsync(ct);

    public Task<List<Recurso>> ListarPorProyectoYTipoAsync(long idProyecto, string tipo, CancellationToken ct) =>
        _db.Recursos
            .Where(r => r.IdProyecto == idProyecto && r.Tipo == tipo)
            .OrderByDescending(r => r.FechaCreacion)
            .AsNoTracking()
            .ToListAsync(ct);

    public Task<List<Recurso>> ListarPorGuionAsync(long idGuion, CancellationToken ct) =>
        _db.Recursos.AsNoTracking()
           .Where(r => r.IdGuion == idGuion)
           .OrderByDescending(r => r.FechaCreacion)
           .ToListAsync(ct);

    public Task<List<Recurso>> ListarPorGuionYTipoAsync(long idGuion, string tipo, CancellationToken ct) =>
        _db.Recursos.AsNoTracking()
           .Where(r => r.IdGuion == idGuion && r.Tipo == tipo)
           .OrderByDescending(r => r.FechaCreacion)
           .ToListAsync(ct);


    public async Task AgregarAsync(Recurso recurso, CancellationToken ct) =>
        await _db.Recursos.AddAsync(recurso, ct);

    public async Task<bool> TryEliminarAsync(long id, CancellationToken ct)
    {
        var refEnVersion = await _db.GuionVersiones
            .AnyAsync(v => v.OutlineRecursoId == id || v.ScriptRecursoId == id, ct);
        var refEnImgs = await _db.GuionImagenes.AnyAsync(i => i.IdImagenRecurso == id, ct);
        var refEnAud = await _db.GuionAudios.AnyAsync(a => a.IdAudioRecurso == id, ct);
        if (refEnVersion || refEnImgs || refEnAud) return false;

        var entidad = await _db.Recursos.FirstOrDefaultAsync(r => r.Id == id, ct);
        if (entidad is null) return true;
        _db.Recursos.Remove(entidad);
        return true;
    }
    public Task GuardarCambiosAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);

  
}
