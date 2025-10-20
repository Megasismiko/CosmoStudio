using CosmoStudio.BLL.Servicios.Interfaces;
using CosmoStudio.Infraestructura.DAL.Repos.Interfaces;
using CosmoStudio.Model;


namespace CosmoStudio.BLL.Servicios.Implementaciones;

public class ProyectoServicio : IProyectoServicio
{
    private readonly IProyectoRepositorio _repo;

    public ProyectoServicio(IProyectoRepositorio repo) => _repo = repo;

    public async Task<Proyecto> CrearAsync(string titulo, string tema, string origen, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(titulo)) throw new ArgumentException("Título requerido");
        if (string.IsNullOrWhiteSpace(tema)) throw new ArgumentException("Tema requerido");
        origen = string.IsNullOrWhiteSpace(origen) ? "Manual" : origen;

        var entidad = new Proyecto
        {            
            Titulo = titulo.Trim(),
            Tema = tema.Trim(),
            Origen = origen.Trim(),
            FechaCreacion = DateTime.UtcNow
        };
        await _repo.CrearAsync(entidad, ct);
        await _repo.GuardarCambiosAsync(ct);
        return entidad;
    }

    public Task<Proyecto?> ObtenerAsync(long id, CancellationToken ct) =>
        _repo.ObtenerPorIdAsync(id, ct);

    public Task<List<Proyecto>> ListarUltimosAsync(int top, CancellationToken ct) =>
        _repo.ListarUltimosAsync(top <= 0 ? 10 : top, ct);

    public Task<List<Proyecto>> BuscarAsync(string texto, int top, CancellationToken ct) =>
        _repo.BuscarPorTemaAsync(texto ?? string.Empty, top <= 0 ? 10 : top, ct);

    public async Task<bool> EliminarAsync(long id, CancellationToken ct)
    {
        await _repo.EliminarAsync(id, ct);
        await _repo.GuardarCambiosAsync(ct);
        return true;
    }
}
