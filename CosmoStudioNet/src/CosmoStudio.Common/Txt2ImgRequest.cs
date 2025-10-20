namespace CosmoStudio.Common
{
    public sealed class Txt2ImgRequest
    {
        public string Prompt { get; set; } = "";
        public string NegativePrompt { get; set; } = "";
        public int Steps { get; set; }
        public double CfgScale { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string SamplerName { get; set; } = "DPM++ 2M";
        public bool EnableHr { get; set; } = false;        // Hires.fix
        public double DenoisingStrength { get; set; } = 0.4;
        public double HrScale { get; set; } = 1.5;         // Upscale factor
        public string? HrUpscaler { get; set; } = "Latent (antialiased)";
        public long Seed { get; set; } = -1;               // -1 = aleatorio
    }
}
