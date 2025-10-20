using CosmoStudio.Infraestructura.DAL.Scaffolding;
using CosmoStudio.Infraestructura.DAL.Repos.Interfaces;
using CosmoStudio.Model;
using Microsoft.EntityFrameworkCore;

namespace CosmoStudio.Infraestructura.DAL.Repos.Implementaciones;

public class GuionRepositorio : IGuionRepositorio
{
    private readonly CosmoDbContext _db;

    public GuionRepositorio(CosmoDbContext db)
    {
        _db = db;
    }

    public Task<Guion?> ObtenerPorProyectoAsync(long idProyecto, CancellationToken ct) =>
        _db.Guiones.AsNoTracking().FirstOrDefaultAsync(g => g.IdProyecto == idProyecto, ct);

    public async Task CrearAsync(Guion guion, CancellationToken ct) =>
        await _db.Guiones.AddAsync(guion, ct);

    public Task ActualizarAsync(Guion guion, CancellationToken ct)
    {
        _db.Guiones.Update(guion);
        return Task.CompletedTask;
    }

    public async Task EliminarPorProyectoAsync(long idProyecto, CancellationToken ct)
    {
        var entidad = await _db.Guiones.FirstOrDefaultAsync(g => g.IdProyecto == idProyecto, ct);
        if (entidad != null)
            _db.Guiones.Remove(entidad);
    }

    public Task GuardarCambiosAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);
}
