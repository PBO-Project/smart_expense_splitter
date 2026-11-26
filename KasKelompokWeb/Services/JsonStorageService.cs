using System;
using System.IO;
using System.Text.Json;

namespace KasKelompok.Services
{
    /// <summary>
    /// Konsep PBO:
    /// - Inheritance: mewarisi BaseStorageService<T>
    /// - Generic Class: mengelola serialisasi/deserialisasi objek tipe T
    /// - System.Text.Json
    /// </summary>
    public class JsonStorageService<T> : BaseStorageService<T> where T : class, new()
    {
        protected static readonly JsonSerializerOptions _jsonOpts = new()
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };

        public JsonStorageService(string filePath = "App_Data/patungan_kelas.json") : base(filePath)
        {
        }

        public override T Load()
        {
            if (!FileExists()) return new T();
            string json = File.ReadAllText(FilePath);
            if (string.IsNullOrWhiteSpace(json)) return new T();
            return JsonSerializer.Deserialize<T>(json, _jsonOpts) ?? new T();
        }

        public override void Save(T data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            string? dir = Path.GetDirectoryName(FilePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            File.WriteAllText(FilePath, JsonSerializer.Serialize(data, _jsonOpts));
        }
    }
}
