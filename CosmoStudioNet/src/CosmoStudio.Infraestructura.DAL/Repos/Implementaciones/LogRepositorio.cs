using CosmoStudio.Infraestructura.DAL.Scaffolding;
using CosmoStudio.Infraestructura.DAL.Repos.Interfaces;
using CosmoStudio.Model;
using Microsoft.EntityFrameworkCore;

namespace CosmoStudio.Infraestructura.DAL.Repos.Implementaciones;

public class LogRepositorio : ILogRepositorio
{
    private readonly CosmoDbContext _db;

    public LogRepositorio(CosmoDbContext db)
    {
        _db = db;
    }

    public async Task AgregarAsync(Log log, CancellationToken ct) =>
        await _db.Logs.AddAsync(log, ct);

    public Task<List<Log>> ListarPorTareaAsync(long idTarea, int top, CancellationToken ct) =>
        _db.Logs
            .Where(l => l.IdTareaRender == idTarea)
            .OrderByDescending(l => l.FechaCreacion)
            .Take(top)
            .AsNoTracking()
            .ToListAsync(ct);

    public Task GuardarCambiosAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);
}
