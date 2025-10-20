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

    public async Task AgregarAsync(Recurso recurso, CancellationToken ct) =>
        await _db.Recursos.AddAsync(recurso, ct);

    public async Task EliminarAsync(long id, CancellationToken ct)
    {
        var entidad = await _db.Recursos.FirstOrDefaultAsync(r => r.Id == id, ct);
        if (entidad is not null)
            _db.Recursos.Remove(entidad);
    }

    public Task GuardarCambiosAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);
}
