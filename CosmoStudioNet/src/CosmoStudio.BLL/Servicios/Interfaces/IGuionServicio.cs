using CosmoStudio.Common;
using CosmoStudio.Model;


namespace CosmoStudio.BLL.Servicios.Interfaces;

public interface IGuionServicio
{
    Task<Guion?> ObtenerPorProyectoAsync(long idProyecto, CancellationToken ct);
    Task<Guion> GuardarRutasAsync(long idProyecto, string rutaOutline, string rutaCompleto, int version, CancellationToken ct);
    Task<bool> EliminarPorProyectoAsync(long idProyecto, CancellationToken ct);

    Task<(string outlinePath, string outline)> GenerarOutlineAsync(long idProyecto, ScriptGenOptions opt, CancellationToken ct);
    Task<(string scriptPath, string script)> GenerarGuionDesdeOutlineAsync(long idProyecto, ScriptGenOptions opt, CancellationToken ct);
    Task<(string scriptPath, string script)> GenerarGuionDesdeOutlinePorSeccionesAsync(long idProyecto, ScriptGenOptions opt, CancellationToken ct);

}
