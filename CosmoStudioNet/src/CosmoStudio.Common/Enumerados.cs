namespace CosmoStudio.Common
{
    public enum OllamaMode
    {
        Borrador,
        Produccion
    }

    public enum OrigenProyecto
    {
        Manual,
        NASA,
        ESA,
        Otro
    }

    public enum TipoRecurso
    {
        Outline,
        Script,
        Imagen,
        Audio,
        Voz,
        Video,
        Otro
    }

    public enum EstadoRecurso
    {
        Draft,
        Active,
        Archived
    }
    public enum EstadoRender
    {
        EnCola,
        EnEjecucion,
        Error,
        Completado
    }

    public enum NivelLog
    {
        Info, 
        Aviso, 
        Error
    }
}
