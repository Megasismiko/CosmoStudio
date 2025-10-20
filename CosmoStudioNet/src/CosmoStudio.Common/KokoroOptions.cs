namespace CosmoStudio.Common;

public class KokoroOptions
{
    public string BaseUrl { get; set; } = "http://localhost:8880/v1";
    public string Model { get; set; } = "kokoro";
    public string Voice { get; set; } = "em_santa";
    public string Format { get; set; } = "wav";
    public double Speed { get; set; } = 1.0;
}
