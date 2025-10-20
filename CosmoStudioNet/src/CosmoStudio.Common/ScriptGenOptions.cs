namespace CosmoStudio.Common
{
    public sealed class ScriptGenOptions
    {
        public ScriptMode Mode { get; set; } = ScriptMode.Borrador;
        public int MinutosObjetivo { get; set; } = 60;
        public int Secciones { get; set; } = 10;          // p.ej. 10 en borrador, 50 en producción
        public int ParrafosPorSeccion { get; set; } = 1;  // p.ej. 1 en borrador, 2-3 en producción
        public int PalabrasPorMinuto { get; set; } = 120; // para calcular longitud aprox
        public string? Estilo { get; set; }               // “calmado”, “documental”, “poético”, etc.
        public string? Modelo { get; set; }               // override de modelo (si quieres)
    }

    public enum ScriptMode
    {
        Borrador,
        Produccion
    }
}
