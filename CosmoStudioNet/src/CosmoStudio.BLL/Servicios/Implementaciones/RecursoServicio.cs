using CosmoStudio.BLL.Servicios.Interfaces;
using CosmoStudio.Common;
using CosmoStudio.Infraestructura.DAL.Repos.Interfaces;
using CosmoStudio.Model;


namespace CosmoStudio.BLL.Servicios.Implementaciones;

public class RecursoServicio : IRecursoServicio
{
    private readonly IRecursoRepositorio _recursos;
    private readonly IProyectoRepositorio _proyectos;

    public RecursoServicio(IRecursoRepositorio recursos, IProyectoRepositorio proyectos)
    {
        _recursos = recursos;
        _proyectos = proyectos;
    }

    public Task<List<Recurso>> ListarAsync(long idProyecto, string? tipo, CancellationToken ct) =>
        string.IsNullOrWhiteSpace(tipo)
            ? _recursos.ListarPorProyectoAsync(idProyecto, ct)
            : _recursos.ListarPorProyectoYTipoAsync(idProyecto, tipo!, ct);

    public async Task<Recurso?> GetByIdAsync(long idRecurso, CancellationToken ct) => await _recursos.GetByIdAsync(idRecurso, ct);


    public async Task<Recurso> AgregarAsync(long idProyecto, TipoRecurso tipo, string storagePath, string? metaJson, CancellationToken ct)
    {
        var proyecto = await _proyectos.ObtenerPorIdAsync(idProyecto, ct)
            ?? throw new InvalidOperationException("Proyecto no encontrado");

        var rec = new Recurso
        {
            IdProyecto = proyecto.Id,
            Tipo = tipo.ToString(),
            Estado = nameof(EstadoRecurso.Active),
            StoragePath = storagePath,
            MetaJson = metaJson,
            FechaCreacion = DateTime.UtcNow
        };

        await _recursos.AgregarAsync(rec, ct);
        await _recursos.GuardarCambiosAsync(ct);
        return rec;
    }

    public async Task<bool> EliminarAsync(long idRecurso, CancellationToken ct)
    {
        await _recursos.TryEliminarAsync(idRecurso, ct);
        await _recursos.GuardarCambiosAsync(ct);
        return true;
    }

    
}
