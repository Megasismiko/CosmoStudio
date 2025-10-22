namespace CosmoStudio.Common.Responses;
public sealed class Txt2ImgResponse
{
    // imágenes en base64 (png por defecto)
    public List<string> Images { get; set; } = new();
    public string? Info { get; set; }
}

