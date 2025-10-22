using CosmoStudio.Infraestructura.DAL.Repos.Interfaces;
using CosmoStudio.Infraestructura.DAL.Scaffolding;
using CosmoStudio.Model;
using Microsoft.EntityFrameworkCore;

namespace CosmoStudio.Infraestructura.DAL.Repos.Implementaciones;

public sealed class GuionVersionRepositorio : IGuionVersionRepositorio
{
    private readonly CosmoDbContext _db;
    public GuionVersionRepositorio(CosmoDbContext db) => _db = db;

    public Task<GuionVersion?> ObtenerAsync(long idVersion, CancellationToken ct) =>
        _db.GuionVersiones
           .Include(v => v.OutlineRecurso)
           .Include(v => v.ScriptRecurso)
           .FirstOrDefaultAsync(v => v.Id == idVersion, ct);

    public Task<List<GuionVersion>> ListarPorGuionAsync(long idGuion, CancellationToken ct) =>
        _db.GuionVersiones
           .Where(v => v.IdGuion == idGuion)
           .OrderByDescending(v => v.NumeroVersion)
           .ToListAsync(ct);

    public async Task<GuionVersion> CrearAsync(long idGuion, int numeroVersion, long? outlineRecursoId, long? scriptRecursoId, string? notas, CancellationToken ct)
    {
        var v = new GuionVersion
        {
            IdGuion = idGuion,
            NumeroVersion = numeroVersion,
            OutlineRecursoId = outlineRecursoId,
            ScriptRecursoId = scriptRecursoId,
            Notas = notas,
            FechaCreacion = DateTime.UtcNow
        };
        await _db.GuionVersiones.AddAsync(v, ct);
        return v;
    }

    public async Task EstablecerComoVigenteAsync(long idGuion, long idVersion, CancellationToken ct)
    {
        // validación de pertenencia
        var pertenece = await _db.GuionVersiones.AnyAsync(v => v.Id == idVersion && v.IdGuion == idGuion, ct);
        if (!pertenece) throw new InvalidOperationException("La versión no pertenece al guion.");

        var g = await _db.Guiones.FirstAsync(x => x.Id == idGuion, ct);
        g.CurrentVersionId = idVersion;
    }

    public async Task SetScriptAsync(long idVersion, long idRecursoScript, CancellationToken ct)
    {
        var v = await _db.GuionVersiones.FirstOrDefaultAsync(x => x.Id == idVersion, ct)
                ?? throw new InvalidOperationException("Versión no encontrada.");
        v.ScriptRecursoId = idRecursoScript;
    }

    public Task GuardarCambiosAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);
}
