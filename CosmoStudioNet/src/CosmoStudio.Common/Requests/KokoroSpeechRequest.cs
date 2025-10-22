
namespace CosmoStudio.Common.Requests;

public sealed record KokoroVoiceMix(
    string name,
    double weight
);

public sealed record KokoroSpeechRequest(
    string model,
    string input,
    IEnumerable<KokoroVoiceMix> voice,   // ← acepta mezcla
    string response_format = "wav",
    double speed = 1.0
);



