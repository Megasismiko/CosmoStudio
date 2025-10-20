namespace CosmoStudio.Common
{
    public class OllamaOptions
    {
        public string BaseUrl { get; set; } = "http://localhost:11434";
        public string DefaultModel { get; set; } = "qwen2.5:32b-instruct-q4_1";
        public double Temperature { get; set; } = 0.6;
    }
}
