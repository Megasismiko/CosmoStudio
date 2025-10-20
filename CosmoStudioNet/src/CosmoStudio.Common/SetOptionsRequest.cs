namespace CosmoStudio.Common
{
    public sealed class SetOptionsRequest
    {
        public string? SdModelCheckpoint { get; set; }    // nombre exacto del checkpoint
        public string? SdVae { get; set; }
        public string? SChurn { get; set; }               // si necesitas tocar scheduler a bajo nivel
                                                          // añade aquí otras opciones del endpoint /sdapi/v1/options si las vas a usar
    }
}
