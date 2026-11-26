namespace KasKelompokWeb.Services;

public interface IRepository<T> where T : class
{
    void Tambah(T item);
    List<T> Semua();
    T? Cari(Func<T, bool> kondisi);
    bool Hapus(Func<T, bool> kondisi);
    int Jumlah { get; }
}