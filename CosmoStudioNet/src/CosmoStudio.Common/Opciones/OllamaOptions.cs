namespace CosmoStudio.Common.Opciones
{
    public class OllamaOptions
    {
        public string BaseUrl { get; set; } = "http://localhost:11434";
        public string DefaultModel { get; set; } = "qwen2.5:32b-instruct-q4_1"; // Producción
        public string DefaultModelDraft { get; set; } = "qwen2.5:14b-instruct-q4_1"; // Borrador
        public double Temperature { get; set; } = 0.6;
       
    }
}
