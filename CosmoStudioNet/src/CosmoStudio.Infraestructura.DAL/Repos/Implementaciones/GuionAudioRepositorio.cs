using CosmoStudio.Infraestructura.DAL.Repos.Interfaces;
using CosmoStudio.Infraestructura.DAL.Scaffolding;
using CosmoStudio.Model;
using Microsoft.EntityFrameworkCore;

namespace CosmoStudio.Infraestructura.DAL.Repos.Implementaciones
{
    public class GuionAudioRepositorio : IGuionAudioRepositorio
    {
        private readonly CosmoDbContext _db;
        public GuionAudioRepositorio(CosmoDbContext db) => _db = db;

        public Task<List<GuionAudio>> ListarPorVersionAsync(long idGuionVersion, CancellationToken ct) =>
            _db.GuionAudios
               .Include(a => a.IdAudioRecursoNavigation)
               .Where(a => a.IdGuionVersion == idGuionVersion)
               .OrderBy(a => a.Orden)
               .AsNoTracking()
               .ToListAsync(ct);

        public async Task<int> ObtenerSiguienteOrdenAsync(long idGuionVersion, CancellationToken ct)
        {
            var max = await _db.GuionAudios
                .Where(a => a.IdGuionVersion == idGuionVersion)
                .MaxAsync(a => (int?)a.Orden, ct);
            return (max ?? 0) + 1;
        }

        public Task AgregarAsync(GuionAudio audio, CancellationToken ct) =>
            _db.GuionAudios.AddAsync(audio, ct).AsTask();

        public async Task ReordenarAsync(long idGuionVersion, IEnumerable<(long id, int nuevoOrden)> ordenes, CancellationToken ct)
        {
            var ids = ordenes.Select(x => x.id).ToList();
            var filas = await _db.GuionAudios.Where(a => a.IdGuionVersion == idGuionVersion && ids.Contains(a.Id)).ToListAsync(ct);
            var map = ordenes.ToDictionary(x => x.id, x => x.nuevoOrden);
            foreach (var f in filas) f.Orden = map[f.Id];
        }

        public async Task EliminarAsync(long id, CancellationToken ct)
        {
            var fila = await _db.GuionAudios.FirstOrDefaultAsync(a => a.Id == id, ct);
            if (fila != null) _db.GuionAudios.Remove(fila);
        }

        public Task GuardarCambiosAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);
    }
}
