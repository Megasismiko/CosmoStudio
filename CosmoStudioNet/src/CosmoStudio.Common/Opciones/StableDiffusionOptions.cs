namespace CosmoStudio.Common.Opciones;

public sealed class StableDiffusionOptions
{
    public string BaseUrl { get; set; } = "http://localhost:7860";
    public string DefaultSampler { get; set; } = "DPM++ 2M";
    public string DefaultScheduler { get; set; } = "Karras";
    public int Steps { get; set; } = 35;
    public double CfgScale { get; set; } = 8.0;
    public int Width { get; set; } = 960;
    public int Height { get; set; } = 540;
    public bool EnableHiresFix { get; set; } = false;
    public double HiresUpscale { get; set; } = 1.5;
    public string? Upscaler { get; set; }
    public string DefaultNegativePrompt { get; set; } = string.Empty;
    public double? DenoisingStrength { get; set; } = 0.45;
    public double? HiresSteps { get; set; } = 0;
} 
