using CosmoStudio.Common.Requests;

namespace CosmoStudio.BLL.Clientes
{
    public interface IOllamaClient
    {
        // ====== Nivel base (bajo nivel) ======

        /// <summary>
        /// Envía una solicitud completa al endpoint /api/generate y devuelve todo el texto generado.
        /// </summary>
        Task<string> GenerarTextoAsync(OllamaGenerateRequest solicitud, CancellationToken ct = default);

       
        // ====== Nivel alto: generación de outline ======

        /// <summary>
        /// Genera el título principal del episodio o guion a partir de un tema dado.
        /// </summary>
        Task<string> GenerarTituloOutlineAsync(string tema, OllamaScriptGenRequest opciones, CancellationToken ct = default);

        /// <summary>
        /// Genera un listado numerado de secciones (outline completo) a partir de un tema.
        /// </summary>
        Task<IReadOnlyList<string>> GenerarOutlineAsync(string tema, OllamaScriptGenRequest opciones, CancellationToken ct = default);

        /// <summary>
        /// Genera el contenido resumido de una sección del outline (1..N) según el tema y el total de secciones.
        /// </summary>
        Task<string> GenerarSeccionOutlineAsync(string tema, int indice, int total, OllamaScriptGenRequest opciones, CancellationToken ct = default);


        // ====== Nivel alto: generación de guion ======

        /// <summary>
        /// Genera el texto completo de una sección del guion, expandiendo los bullets en párrafos naturales.
        /// </summary>
        Task<string> GenerarSeccionGuionAsync(string tituloSeccion, IReadOnlyList<string> bullets, int palabrasObjetivo, OllamaScriptGenRequest opciones, CancellationToken ct = default);


        // ====== Nivel alto: prompts para imágenes ======

        /// <summary>
        /// Convierte el texto de una sección (en español) en un prompt visual en inglés optimizado para Stable Diffusion.
        /// </summary>
        Task<string> GenerarPromptImagenDesdeSeccionAsync(string textoSeccion, CancellationToken ct = default);


        // ====== Nivel alto: revisión y corrección de guiones ======

        /// <summary>
        /// Revisa un guion completo detectando errores lingüísticos, caracteres extraños o palabras fuera de contexto
        /// en relación con el tema tratado, devolviendo una versión corregida y limpia.
        /// </summary>
        Task<string> RevisarYCorregirGuionAsync(string guionMarkdown, string tema, CancellationToken ct = default);


        Task<string> TraducirAsync(string textoOrigen, string a = "Spanish (es-ES)", bool produccion = true, CancellationToken ct = default);
    }
}
