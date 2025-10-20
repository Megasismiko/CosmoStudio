namespace CosmoStudio.Infraestructura.TTS
{   
    public sealed record KokoroSpeechRequest(
      string model,
      string input,
      string voice,
      string response_format = "wav",
      double speed = 1
  );
}
