using System.Text.Json.Serialization;

namespace CosmoStudio.BLL.Servicios.Interfaces;

public interface IRenderServicio
{
    Task<(string manifestPath, RenderManifest manifest)> GenerarManifestAsync(long idProyecto, int fps = 30, string resolution = "1920x1080", CancellationToken ct = default);
    Task<string> RenderProyectoVigenteAsync(long idProyecto, CancellationToken ct = default);

    Task<string> RenderDesdeManifestAsync(string manifestPath, CancellationToken ct = default);
}

public sealed class RenderManifest
{
    [JsonPropertyName("projectId")] public long ProjectId { get; set; }
    [JsonPropertyName("version")] public int Version { get; set; }
    [JsonPropertyName("fps")] public int Fps { get; set; }
    [JsonPropertyName("resolution")] public string Resolution { get; set; } = "1920x1080";
    [JsonPropertyName("music")] public string? Music { get; set; }
    [JsonPropertyName("sections")] public List<RenderSection> Sections { get; set; } = new();
}

public sealed class RenderSection
{
    [JsonPropertyName("index")] public int Index { get; set; }
    [JsonPropertyName("title")] public string? Title { get; set; }
    [JsonPropertyName("text")] public string? Text { get; set; }
    [JsonPropertyName("audio")] public string? Audio { get; set; }
    [JsonPropertyName("image")] public string? Image { get; set; }
    [JsonPropertyName("duration")] public double? Duration { get; set; }
    [JsonPropertyName("transitionIn")] public Transition? TransitionIn { get; set; }
    [JsonPropertyName("transitionOut")] public Transition? TransitionOut { get; set; }
    [JsonPropertyName("kenBurns")] public KenBurns? KenBurns { get; set; }
    [JsonPropertyName("overlayTitle")] public OverlayTitle? OverlayTitle { get; set; }
}

public sealed class Transition
{
    [JsonPropertyName("type")] public string Type { get; set; } = "fade";
    [JsonPropertyName("duration")] public double Duration { get; set; } = 0.5;
}

public sealed class KenBurns
{
    [JsonPropertyName("mode")] public string Mode { get; set; } = "auto";
    [JsonPropertyName("zoom")] public double Zoom { get; set; } = 1.06;
    [JsonPropertyName("pan")] public string Pan { get; set; } = "right"; // left/right/none
}

public sealed class OverlayTitle
{
    [JsonPropertyName("enabled")] public bool Enabled { get; set; } = true;
    [JsonPropertyName("title")] public string? Title { get; set; }
    [JsonPropertyName("showAt")] public double ShowAt { get; set; } = 0.5;
    [JsonPropertyName("duration")] public double Duration { get; set; } = 2.0;
}
