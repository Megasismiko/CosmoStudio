using System.Text.Json.Serialization;

namespace CosmoStudio.Common.Requests;

// ==== REQUEST ====
public sealed class OllamaGenerateRequest
{
    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;

    [JsonPropertyName("prompt")]
    public string Prompt { get; set; } = string.Empty;

    [JsonPropertyName("stream")]
    public bool Stream { get; set; } = false;

    [JsonPropertyName("options")]
    public OllamaOptionsBlock Options { get; set; } = new();

    // Mantén vivo el modelo en RAM (segundos) después de la llamada.
    [JsonPropertyName("keep_alive")]
    public int KeepAlive { get; set; } = 600;

    // Campos opcionales útiles (no todos los servers los usan, pero no molestan):
    [JsonPropertyName("system")]
    public string? System { get; set; }  // prompt de sistema (plantilla/rol)

    [JsonPropertyName("template")]
    public string? Template { get; set; } // plantilla de render del prompt

    // Para conversación/contexto (si lo necesitas en el futuro)
    [JsonPropertyName("context")]
    public int[]? Context { get; set; }

    // Secuencias de parada de alto nivel (aplica además de Options.Stop si lo prefieres)
    [JsonPropertyName("stop")]
    public string[]? Stop { get; set; }
}

// ==== OPTIONS ====
public sealed class OllamaOptionsBlock
{
    [JsonPropertyName("num_predict")]
    public int NumPredict { get; set; }          // máx. tokens a generar

    [JsonPropertyName("num_ctx")]
    public int NumCtx { get; set; }              // ventana de contexto

    [JsonPropertyName("num_batch")]
    public int NumBatch { get; set; }            // tamaño de lote

    [JsonPropertyName("temperature")]
    public double Temperature { get; set; } = 0.7;

    [JsonPropertyName("top_p")]
    public double TopP { get; set; } = 0.9;

    [JsonPropertyName("top_k")]
    public int TopK { get; set; } = 40;

    [JsonPropertyName("repeat_penalty")]
    public double RepeatPenalty { get; set; } = 1.1;

    [JsonPropertyName("stop")]
    public string[]? Stop { get; set; }          // stop sequences a nivel options
}
