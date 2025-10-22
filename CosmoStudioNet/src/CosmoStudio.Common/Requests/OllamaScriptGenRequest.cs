namespace CosmoStudio.Common.Requests;

public sealed class OllamaScriptGenRequest
{
    public OllamaMode Mode { get; set; } = OllamaMode.Borrador;
    public int Secciones { get; set; } = 5;
    public int MinutosObjetivo { get; set; } = 5;
    public int PalabrasPorMinuto { get; set; } = 130;

}
