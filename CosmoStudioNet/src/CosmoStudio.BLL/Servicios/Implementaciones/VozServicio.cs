using CosmoStudio.BLL.Kokoro;
using CosmoStudio.BLL.Servicios.Interfaces;
using CosmoStudio.Common;
using CosmoStudio.Model;
using Microsoft.Extensions.Options;
using System.Net.Sockets;


namespace CosmoStudio.BLL.Servicios.Implementaciones;

public sealed class VozServicio : IVozServicio
{
    private readonly IGuionServicio _guiones;
    private readonly IRecursoServicio _recursos;
    private readonly IkokoroClient _kokoroClient;    
    private readonly KokoroOptions _kokoroOptions;
    private readonly StorageOptions _storageOptions;

    public VozServicio(IGuionServicio guiones, IRecursoServicio recursos, IkokoroClient kokoroClient, IOptions<KokoroOptions> kokoroOptions, IOptions<StorageOptions> storageOptions)
    {
        _guiones = guiones;
        _recursos = recursos;
        _kokoroClient = kokoroClient;        
        _kokoroOptions = kokoroOptions.Value;
        _storageOptions = storageOptions.Value;
    }

    public async Task<long> GenerarAudioProyectoAsync(long idProyecto, CancellationToken ct = default)
    {
        // 1) Obtener guion del proyecto
        var guion = await _guiones.ObtenerPorProyectoAsync(idProyecto, ct)
            ?? throw new InvalidOperationException($"Proyecto {idProyecto} no tiene guion.");

        var texto = await File.ReadAllTextAsync(guion.RutaCompleto, ct);

        // 2) Sintetizar (usa opciones de appsettings por defecto)
        //    Si estás en MP3 y no quieres troceo, pon MaxChars alto en appsettings (ya lo tienes a 10000)
        var audioBytes = await _kokoroClient.SynthesizeAsync(texto,ct: ct);

        // 3) Carpeta de salida consistente con RunsRoot
        var runDir = Path.Combine(_storageOptions.RunsRoot, idProyecto.ToString());
        Directory.CreateDirectory(runDir);

        // Extensión según formato configurado (mp3/wav)
        var ext = _kokoroOptions.Format?.Equals("mp3", StringComparison.OrdinalIgnoreCase) == true ? "mp3" : "wav";
        var fileName = $"tts_{DateTime.UtcNow:yyyyMMdd_HHmmss}.{ext}";
        var ruta = Path.Combine(runDir, fileName);

        await File.WriteAllBytesAsync(ruta, audioBytes, ct);

        // 4) Registrar Recurso vía servicio (guarda cambios internamente)
        var meta = System.Text.Json.JsonSerializer.Serialize(new
        {
            _kokoroOptions.Voice,
            _kokoroOptions.Model,
            _kokoroOptions.Format
        });


        var recurso = await _recursos.AgregarAsync(idProyecto, TipoRecurso.Voz.ToString(), ruta, meta, ct);


        return recurso.Id;
    }
}
