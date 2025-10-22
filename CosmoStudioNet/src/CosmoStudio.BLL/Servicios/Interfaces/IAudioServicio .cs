namespace CosmoStudio.BLL.Servicios.Interfaces
{
    public interface IAudioServicio
    {
        Task<long> GenerarVozDesdeGuionAsync(long idProyecto, CancellationToken ct = default);
        Task<IReadOnlyList<long>> GenerarVozDesdeGuionPorSeccionesAsync(long idProyecto, CancellationToken ct = default);
    }
}
