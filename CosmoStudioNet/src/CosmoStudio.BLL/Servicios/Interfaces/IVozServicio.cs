namespace CosmoStudio.BLL.Servicios.Interfaces
{
    public interface IVozServicio
    {
        Task<long> GenerarAudioProyectoAsync(long idProyecto, CancellationToken ct = default);
    }
}
