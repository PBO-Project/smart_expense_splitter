namespace KasKelompokWeb.Services;

public class Repository<T> : IRepository<T> where T : class
{
    private readonly List<T> _items = new();

    public int Jumlah => _items.Count;

    public void Tambah(T item) => _items.Add(item);

    public List<T> Semua() => _items;

    public T? Cari(Func<T, bool> kondisi) => _items.FirstOrDefault(kondisi);

    public bool Hapus(Func<T, bool> kondisi)
    {
        var item = _items.FirstOrDefault(kondisi);
        return item != null && _items.Remove(item);
    }
}