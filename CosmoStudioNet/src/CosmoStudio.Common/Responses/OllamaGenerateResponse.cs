// ==== RESPONSES ====
namespace CosmoStudio.Common.Responses;

using System.Text.Json.Serialization;

// Respuesta “no streaming”
public sealed class OllamaGenerateResponse
{
    [JsonPropertyName("model")] public string? Model { get; set; }
    [JsonPropertyName("created_at")] public DateTimeOffset CreatedAt { get; set; }
    [JsonPropertyName("response")] public string Response { get; set; } = string.Empty;
    [JsonPropertyName("done")] public bool Done { get; set; }
}

// Chunk NDJSON para streaming
public sealed class OllamaStreamChunk
{
    [JsonPropertyName("response")] public string? Response { get; set; }
    [JsonPropertyName("done")] public bool Done { get; set; }
}
