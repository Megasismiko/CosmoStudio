using CosmoStudio.BLL.Clientes;

using CosmoStudio.BLL.Servicios.Interfaces;
using CosmoStudio.Common;
using CosmoStudio.Common.Requests;
using CosmoStudio.Infraestructura.DAL.Repos.Interfaces;
using CosmoStudio.Model;
using System.Text;

namespace CosmoStudio.BLL.Servicios.Implementaciones
{
    public class GuionServicio : IGuionServicio
    {
        private readonly IGuionRepositorio _guiones;
        private readonly IGuionVersionRepositorio _versiones;
        private readonly IRecursoServicio _recursos;
        private readonly IProyectoRepositorio _proyectos;
        private readonly IOllamaClient _ollama;
        private readonly ILocalFileStorage _fileStorage;

        public GuionServicio(
            IGuionRepositorio guiones,
            IGuionVersionRepositorio versiones,
            IRecursoServicio recursos,
            IProyectoRepositorio proyectos,
            IOllamaClient ollama,
            ILocalFileStorage fileStorage)
        {
            _guiones = guiones;
            _versiones = versiones;
            _recursos = recursos;
            _proyectos = proyectos;
            _ollama = ollama;
            _fileStorage = fileStorage;
        }

        public Task<Guion?> ObtenerPorProyectoAsync(long idProyecto, CancellationToken ct) =>
            _guiones.ObtenerPorProyectoAsync(idProyecto, ct);

        public async Task<bool> EliminarPorProyectoAsync(long idProyecto, CancellationToken ct)
        {
            await _guiones.EliminarPorProyectoAsync(idProyecto, ct);
            await _guiones.GuardarCambiosAsync(ct);
            return true;
        }

        // =============================================================
        // ========== GENERACIÓN DE OUTLINE ============================
        // =============================================================

        public async Task<(string ruta, string texto)> GenerarOutlineAsync(long idProyecto, OllamaScriptGenRequest opciones, CancellationToken ct)
        {
            var proyecto = await _proyectos.ObtenerPorIdAsync(idProyecto, ct)
                ?? throw new InvalidOperationException("Proyecto no encontrado");

            // Asegurar guion existente
            var guion = await _guiones.ObtenerPorProyectoAsync(idProyecto, ct);
            if (guion is null)
            {
                guion = new Guion { IdProyecto = idProyecto, FechaCreacion = DateTime.UtcNow };
                await _guiones.CrearAsync(guion, ct);
                await _guiones.GuardarCambiosAsync(ct);
            }

            // Calcular próxima versión
            var versiones = await _versiones.ListarPorGuionAsync(guion.Id, ct);
            var numeroVersion = (versiones.FirstOrDefault()?.NumeroVersion ?? 0) + 1;

            // Generar título
            var titulo = (await _ollama.GenerarTituloOutlineAsync(proyecto.Tema, opciones, ct)).Trim();

            // Generar secciones del outline
            var total = opciones.Mode == OllamaMode.Produccion
                ? Math.Max(opciones.Secciones, 50)
                : Math.Max(opciones.Secciones, 10);

            var secciones = new List<string>();
            for (int i = 1; i <= total; i++)
            {
                ct.ThrowIfCancellationRequested();
                var seccion = await _ollama.GenerarSeccionOutlineAsync(proyecto.Tema, i, total, opciones, ct);
                secciones.Add(seccion.Trim());
            }

            // Ensamblar texto final
            var sb = new StringBuilder();
            sb.AppendLine(titulo).AppendLine();
            foreach (var s in secciones)
                sb.AppendLine(s).AppendLine();
            sb.AppendLine("Fuentes sugeridas:")
              .AppendLine("- ").AppendLine("- ").AppendLine("- ");

            var outlineTexto = sb.ToString();

            // Guardar en disco
            var outlinePath = await _fileStorage.SaveTextAsync(
                idProyecto: idProyecto,
                version: numeroVersion,
                tipo: TipoRecurso.Outline,
                content: outlineTexto,
                extension: "md",
                index: null,
                ct: ct);

            // Registrar recurso y versión
            var outlineRec = await _recursos.AgregarAsync(idProyecto, TipoRecurso.Outline, outlinePath, metaJson: null, ct);
            var version = await _versiones.CrearAsync(guion.Id, numeroVersion, outlineRec.Id, null, "auto: outline", ct);
            await _versiones.GuardarCambiosAsync(ct);
            await _versiones.EstablecerComoVigenteAsync(guion.Id, version.Id, ct);
            await _versiones.GuardarCambiosAsync(ct);

            return (outlinePath, outlineTexto);
        }

        // =============================================================
        // ========== GENERACIÓN DE GUION ==============================
        // =============================================================

        public async Task<(string ruta, string texto)> GenerarGuionDesdeOutlineAsync(long idProyecto, OllamaScriptGenRequest opciones, CancellationToken ct)
        {
            var proyecto = await _proyectos.ObtenerPorIdAsync(idProyecto, ct)
                ?? throw new InvalidOperationException("Proyecto no encontrado");

            var guion = await _guiones.ObtenerPorProyectoAsync(proyecto.Id, ct)
                ?? throw new InvalidOperationException("No existe guion para este proyecto");

            var version = guion.CurrentVersion ?? throw new InvalidOperationException("No hay versión vigente del guion.");

            var outlinePath = version.OutlineRecurso?.StoragePath
                ?? throw new InvalidOperationException("La versión vigente no tiene Outline asociado.");

            var outline = await File.ReadAllTextAsync(outlinePath, ct);
            var secciones = ParsearOutline(outline);
            if (secciones.Count == 0)
                throw new InvalidOperationException("Outline sin secciones parseables");

            // Calcular palabras por sección
            var wpm = Math.Clamp(opciones.PalabrasPorMinuto, 90, 180);
            var totalPalabras = opciones.MinutosObjetivo * wpm;
            var palabrasPorSeccion = Math.Max(120, totalPalabras / Math.Max(1, secciones.Count));

            // Construcción del script
            var sb = new StringBuilder();
            sb.AppendLine($"# {proyecto.Titulo}").AppendLine();

            var minuto = 0;
            foreach (var s in secciones)
            {
                ct.ThrowIfCancellationRequested();
                var texto = await _ollama.GenerarSeccionGuionAsync(s.Titulo, s.Puntos, palabrasPorSeccion, opciones, ct);
                sb.AppendLine($"## {s.Titulo}  —  ~[{minuto:00}:00]");
                sb.AppendLine(texto.Trim()).AppendLine();
                minuto += Math.Max(1, opciones.MinutosObjetivo / Math.Max(1, secciones.Count));
            }

            var scriptTexto = sb.ToString();

            // Guardar script
            var scriptPath = await _fileStorage.SaveTextAsync(
                idProyecto: idProyecto,
                version: version.NumeroVersion,
                tipo: TipoRecurso.Script,
                content: scriptTexto,
                extension: "md",
                index: null,
                ct: ct);

            // Registrar recurso y vincularlo a la versión vigente
            var scriptRec = await _recursos.AgregarAsync(idProyecto, TipoRecurso.Script, scriptPath, metaJson: null, ct);
            await _versiones.SetScriptAsync(version.Id, scriptRec.Id, ct);
            await _versiones.GuardarCambiosAsync(ct);

            return (scriptPath, scriptTexto);
        }

        // =============================================================
        // ========== PARSER DE OUTLINE ================================
        // =============================================================

        private static List<(string Titulo, List<string> Puntos)> ParsearOutline(string outline)
        {
            var resultado = new List<(string, List<string>)>();
            var lineas = outline.Split('\n').Select(l => l.Trim()).ToList();

            string? tituloActual = null;
            var bulletsActuales = new List<string>();

            foreach (var linea in lineas)
            {
                if (System.Text.RegularExpressions.Regex.IsMatch(linea, @"^\d+[\)\.]\s"))
                {
                    if (tituloActual != null)
                        resultado.Add((tituloActual, new List<string>(bulletsActuales)));

                    tituloActual = System.Text.RegularExpressions.Regex.Replace(linea, @"^\d+[\)\.]\s*", "");
                    bulletsActuales.Clear();
                }
                else if (linea.StartsWith("- "))
                {
                    bulletsActuales.Add(linea.Substring(2));
                }
            }

            if (tituloActual != null)
                resultado.Add((tituloActual, bulletsActuales));

            return resultado;
        }
    }
}
