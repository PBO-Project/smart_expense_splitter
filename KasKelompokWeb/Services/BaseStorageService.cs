using System;
using System.IO;

namespace KasKelompok.Services
{
    /// <summary>
    /// Konsep PBO:
    /// - Abstract Class
    /// - Mengimplementasikan Generic Interface IStorageService<T>
    /// - Variable get & set dengan validasi
    /// </summary>
    public abstract class BaseStorageService<T> : IStorageService<T> where T : class, new()
    {
        private string _filePath = string.Empty;

        // Variable get & set method dengan validasi
        public string FilePath
        {
            get => _filePath;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Path file penyimpanan tidak boleh kosong.");
                _filePath = value;
            }
        }

        protected BaseStorageService(string filePath)
        {
            FilePath = filePath;
        }

        public virtual bool FileExists()
        {
            return File.Exists(FilePath);
        }

        public abstract T Load();
        public abstract void Save(T data);
    }
}
