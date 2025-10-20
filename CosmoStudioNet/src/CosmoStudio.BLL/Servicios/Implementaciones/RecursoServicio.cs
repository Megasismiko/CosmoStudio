using CosmoStudio.BLL.Servicios.Interfaces;
using CosmoStudio.Infraestructura.DAL.Repos.Interfaces;
using CosmoStudio.Model;


namespace CosmoStudio.BLL.Servicios.Implementaciones;

public class RecursoServicio : IRecursoServicio
{
    private readonly IRecursoRepositorio _repo;
    private readonly IProyectoRepositorio _proyectos;

    public RecursoServicio(IRecursoRepositorio repo, IProyectoRepositorio proyectos)
    {
        _repo = repo;
        _proyectos = proyectos;
    }

    public Task<List<Recurso>> ListarAsync(long idProyecto, string? tipo, CancellationToken ct) =>
        string.IsNullOrWhiteSpace(tipo)
            ? _repo.ListarPorProyectoAsync(idProyecto, ct)
            : _repo.ListarPorProyectoYTipoAsync(idProyecto, tipo!, ct);

    public async Task<Recurso> AgregarAsync(long idProyecto, string tipo, string ruta, string? metaJson, CancellationToken ct)
    {
        var proyecto = await _proyectos.ObtenerPorIdAsync(idProyecto, ct)
            ?? throw new InvalidOperationException("Proyecto no encontrado");

        var rec = new Recurso
        {            
            IdProyecto = proyecto.Id,
            Tipo = tipo,
            Ruta = ruta,
            MetaJson = metaJson,
            FechaCreacion = DateTime.UtcNow
        };

        await _repo.AgregarAsync(rec, ct);
        await _repo.GuardarCambiosAsync(ct);
        return rec;
    }

    public async Task<bool> EliminarAsync(long idRecurso, CancellationToken ct)
    {
        await _repo.EliminarAsync(idRecurso, ct);
        await _repo.GuardarCambiosAsync(ct);
        return true;
    }
}
