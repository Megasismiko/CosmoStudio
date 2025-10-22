using System.Text.Json.Serialization;

namespace CosmoStudio.Common.Requests;
public sealed class Txt2ImgRequest
{
    [JsonPropertyName("prompt")] public string Prompt { get; set; } = string.Empty;

    [JsonPropertyName("negative_prompt")] public string? NegativePrompt { get; set; }

    [JsonPropertyName("steps")] public int Steps { get; set; }

    [JsonPropertyName("sampler_name")] public string? SamplerName { get; set; }

    [JsonPropertyName("cfg_scale")] public double CfgScale { get; set; }

    [JsonPropertyName("seed")] public long Seed { get; set; } = -1;

    [JsonPropertyName("width")] public int Width { get; set; }

    [JsonPropertyName("height")] public int Height { get; set; }

    [JsonPropertyName("scheduler")] public string? Scheduler { get; set; }
      

    // HIRES FIX
    [JsonPropertyName("enable_hr")] public bool EnableHr { get; set; }

    [JsonPropertyName("hr_scale")] public double HrScale { get; set; }

    [JsonPropertyName("hr_upscaler")] public string? HrUpscaler { get; set; }

    [JsonPropertyName("denoising_strength")] public double? DenoisingStrength { get; set; }

    [JsonPropertyName("hr_second_pass_steps")] public double? HiresSteps { get; set; }


}
