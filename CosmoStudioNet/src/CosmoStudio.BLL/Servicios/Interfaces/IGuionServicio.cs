using CosmoStudio.Common.Requests;
using CosmoStudio.Model;

namespace CosmoStudio.BLL.Servicios.Interfaces;

public interface IGuionServicio
{
    Task<Guion?> ObtenerPorProyectoAsync(long idProyecto, CancellationToken ct);
    Task<bool> EliminarPorProyectoAsync(long idProyecto, CancellationToken ct);
    Task<(string ruta, string texto)> GenerarOutlineAsync(long idProyecto, OllamaScriptGenRequest opciones, CancellationToken ct);
    Task<(string ruta, string texto)> GenerarGuionDesdeOutlineAsync(long idProyecto, OllamaScriptGenRequest opciones, CancellationToken ct);

}
