using CosmoStudio.Common;

namespace CosmoStudio.BLL.Ollama;

/// <summary>
/// Cliente de LLM para generación de outline y guion, con modos (borrador/producción).
/// </summary>
public interface IOllamaClient
{
    /// <summary>
    /// Llamada cruda al endpoint de generación (útil para pruebas o uso avanzado).
    /// </summary>
    /// <param name="model">Nombre del modelo (si vacío, la impl usará el default de configuración).</param>
    /// <param name="prompt">Prompt completo a enviar al LLM.</param>
    /// <param name="stream">Si true, solicita streaming (la impl puede ignorarlo si no lo soporta).</param>
    /// <param name="ct">Token de cancelación.</param>
    Task<string> GenerateAsync(string model, string prompt, bool stream, CancellationToken ct);

    /// <summary>
    /// Genera un OUTLINE detallado en función de las opciones (modo, secciones, estilo…).
    /// </summary>
    /// <param name="tema">Tema central del episodio.</param>
    /// <param name="opt">Opciones de generación (modo, minutos objetivo, etc.).</param>
    /// <param name="ct">Token de cancelación.</param>
    Task<string> GenerateOutlineAsync(string tema, ScriptGenOptions opt, CancellationToken ct);

    /// <summary>
    /// Genera el GUIÓN completo a partir de un outline y opciones (modo, párrafos por sección, etc.).
    /// </summary>
    /// <param name="outline">Outline previo en texto.</param>
    /// <param name="opt">Opciones de generación (modo, minutos objetivo, etc.).</param>
    /// <param name="ct">Token de cancelación.</param>
    Task<string> GenerateScriptFromOutlineAsync(string outline, ScriptGenOptions opt, CancellationToken ct);
      
    Task<string> GenerateSectionAsync(
        string sectionTitle,
        IReadOnlyList<string> bullets,
        int targetWords,
        ScriptGenOptions opt,
        CancellationToken ct
    );
}
