namespace CosmoStudio.Common.Opciones;

public class KokoroVoiceOption
{
    public string Name { get; set; } = "em_santa";
    public double Weight { get; set; } = 1;
}


public class KokoroOptions
{
    public string BaseUrl { get; set; } = "http://localhost:8880/v1";
    public string Model { get; set; } = "kokoro";
    public IEnumerable<KokoroVoiceOption> Voice { get; set; } = [
        new KokoroVoiceOption() {Name= "em_santa",Weight = 0.5 },
        new KokoroVoiceOption() { Name = "em_alex", Weight = 0.5 }
    ];
    public string Format { get; set; } = "wav";
    public double Speed { get; set; } = 1.0;
}
