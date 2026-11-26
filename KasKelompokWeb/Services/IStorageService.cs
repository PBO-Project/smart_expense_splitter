namespace KasKelompok.Services
{
    /// <summary>
    /// Konsep PBO: Generic Interface
    /// Interface yang menerima tipe T secara dinamis (fleksibel untuk model apa pun)
    /// </summary>
    public interface IStorageService<T> where T : class, new()
    {
        string FilePath { get; set; }
        T Load();
        void Save(T data);
        bool FileExists();
    }
}
