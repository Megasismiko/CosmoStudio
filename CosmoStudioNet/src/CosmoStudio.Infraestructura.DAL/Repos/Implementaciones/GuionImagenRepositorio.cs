using CosmoStudio.Infraestructura.DAL.Repos.Interfaces;
using CosmoStudio.Infraestructura.DAL.Scaffolding;
using CosmoStudio.Model;
using Microsoft.EntityFrameworkCore;

namespace CosmoStudio.Infraestructura.DAL.Repos.Implementaciones
{
    public class GuionImagenRepositorio : IGuionImagenRepositorio
    {
        private readonly CosmoDbContext _db;
        public GuionImagenRepositorio(CosmoDbContext db) => _db = db;

        public Task<List<GuionImagen>> ListarPorVersionAsync(long idGuionVersion, CancellationToken ct) =>
            _db.GuionImagenes
               .Include(i => i.IdImagenRecursoNavigation)
               .Where(i => i.IdGuionVersion == idGuionVersion)
               .OrderBy(i => i.Orden)
               .AsNoTracking()
               .ToListAsync(ct);

        public async Task<int> ObtenerSiguienteOrdenAsync(long idGuionVersion, CancellationToken ct)
        {
            var max = await _db.GuionImagenes
                .Where(i => i.IdGuionVersion == idGuionVersion)
                .MaxAsync(i => (int?)i.Orden, ct);
            return (max ?? 0) + 1;
        }

        public Task AgregarAsync(GuionImagen imagen, CancellationToken ct) =>
            _db.GuionImagenes.AddAsync(imagen, ct).AsTask();

        public async Task ReordenarAsync(long idGuionVersion, IEnumerable<(long id, int nuevoOrden)> ordenes, CancellationToken ct)
        {
            var ids = ordenes.Select(x => x.id).ToList();
            var filas = await _db.GuionImagenes.Where(i => i.IdGuionVersion == idGuionVersion && ids.Contains(i.Id)).ToListAsync(ct);
            var map = ordenes.ToDictionary(x => x.id, x => x.nuevoOrden);
            foreach (var f in filas) f.Orden = map[f.Id];
        }

        public async Task EliminarAsync(long id, CancellationToken ct)
        {
            var fila = await _db.GuionImagenes.FirstOrDefaultAsync(i => i.Id == id, ct);
            if (fila != null) _db.GuionImagenes.Remove(fila);
        }

        public Task GuardarCambiosAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);
    }
}