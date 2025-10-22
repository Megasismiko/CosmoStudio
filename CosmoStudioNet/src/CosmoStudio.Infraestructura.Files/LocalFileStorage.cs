using CosmoStudio.BLL.Servicios.Interfaces;
using CosmoStudio.Common;
using CosmoStudio.Common.Opciones;
using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;

namespace CosmoStudio.Infraestructura.Files
{
    public class LocalFileStorage : ILocalFileStorage
    {
        private readonly StorageOptions _storageOptions;

        public LocalFileStorage(IOptions<StorageOptions> opt) => _storageOptions = opt.Value;

        /// <summary>
        /// Guarda bytes construyendo ruta y nombre según {runs}/{proyecto}/v{version}/{tipo}/
        /// y nombre {TipoRecurso}_v{version}{_{index?}}.{extension}
        /// Devuelve el path absoluto del archivo guardado.
        /// </summary>
        public async Task<string> SaveBytesAsync(
            long idProyecto,
            int version,
            TipoRecurso tipo,
            ReadOnlyMemory<byte> data,
            string extension,
            int? index = null,
            CancellationToken ct = default)
        {
            var dir = GetResourceDirectory(idProyecto, version, tipo);
            Directory.CreateDirectory(dir);

            var fileName = BuildFileName(tipo, version, extension, index);
            var path = Path.Combine(dir, fileName);

            // Evita sobrescrituras si ya existe
            path = EnsureUniquePath(path);

            await File.WriteAllBytesAsync(path, data.ToArray(), ct);
            return path;
        }

        /// <summary>
        /// Guarda texto con la misma convención que SaveBytesAsync.
        /// </summary>
        public async Task<string> SaveTextAsync(
            long idProyecto,
            int version,
            TipoRecurso tipo,
            string content,
            string extension,
            int? index = null,
            CancellationToken ct = default)
        {
            var dir = GetResourceDirectory(idProyecto, version, tipo);
            Directory.CreateDirectory(dir);

            var fileName = BuildFileName(tipo, version, extension, index);
            var path = Path.Combine(dir, fileName);

            path = EnsureUniquePath(path);

            await File.WriteAllTextAsync(path, content, ct);
            return path;
        }

        /// <summary>
        /// runs/{idProyecto}/v{version}/{text|audio|video}
        /// </summary>
        public string GetResourceDirectory(long idProyecto, int version, TipoRecurso tipo)
        {
            var tipoFolder = MapTipoFolder(tipo, _storageOptions);
            return Path.Combine(_storageOptions.RunsRoot, idProyecto.ToString(), $"v{version}", tipoFolder);
        }

        public string GetFilePath(long idProyecto, int version, TipoRecurso tipo, string fileName)
        {
            return Path.Combine(GetResourceDirectory(idProyecto, version, tipo), SanitizeFileName(fileName));
        }

        // ===== Helpers =====

        private string BuildFileName(TipoRecurso tipo, int version, string extension, int? index)
        {
            var ext = NormalizeExtension(extension);
            var tipoStr = tipo.ToString(); // si prefieres minúsculas: .ToLowerInvariant()
            var idxPart = index.HasValue ? $"_{index.Value:D2}" : string.Empty;
            var raw = $"{tipoStr}_v{version}{idxPart}.{ext}";
            return SanitizeFileName(raw);
        }

        private static string NormalizeExtension(string extension)
        {
            var ext = extension?.Trim().TrimStart('.') ?? "";
            return string.IsNullOrWhiteSpace(ext) ? "bin" : ext.ToLowerInvariant();
        }

        private static string MapTipoFolder(TipoRecurso tipo, StorageOptions _storageOptions)
        {
            // Ajusta según tu enum TipoRecurso
            // p.ej.: Texto, Voz, Video, Imagen, etc.
            return tipo switch
            {
                TipoRecurso.Outline => _storageOptions.TextDir,
                TipoRecurso.Script => _storageOptions.TextDir,
                TipoRecurso.Voz => _storageOptions.AudioDir,
                TipoRecurso.Imagen => _storageOptions.ImagesDir,
                TipoRecurso.Video => _storageOptions.VideoDir,
                _ => "misc"
            };
        }

        private static string SanitizeFileName(string name)
        {
            var invalid = string.Join("", Path.GetInvalidFileNameChars());
            var rx = new Regex($"[{Regex.Escape(invalid)}]");
            var cleaned = rx.Replace(name, "_");
            cleaned = Regex.Replace(cleaned, @"\s+", "_").Trim('_');
            return cleaned.Length == 0 ? "file" : cleaned;
        }

        private static string EnsureUniquePath(string path)
        {
            if (!File.Exists(path)) return path;

            var dir = Path.GetDirectoryName(path)!;
            var name = Path.GetFileNameWithoutExtension(path);
            var ext = Path.GetExtension(path);

            int i = 2;
            string candidate;
            do
            {
                candidate = Path.Combine(dir, $"{name}_{i:D2}{ext}");
                i++;
            } while (File.Exists(candidate));

            return candidate;
        }
    }
}