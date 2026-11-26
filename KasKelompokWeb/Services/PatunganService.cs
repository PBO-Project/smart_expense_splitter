using KasKelompokWeb.Models;

namespace KasKelompokWeb.Services;

public class PatunganService : IPatunganService
{
    private readonly IRepository<Siswa> _siswa = new Repository<Siswa>();
    private readonly IRepository<Pengeluaran> _tagihan = new Repository<Pengeluaran>();

    // Dictionary: key = (idTagihan, idSiswa)
    private readonly Dictionary<(int idTagihan, int idSiswa), StatusBayar> _status = new();

    private int _nextSiswaId = 1;
    private int _nextTagihanId = 1;

    public Siswa TambahSiswa(string nama, string npm)
    {
        if (_siswa.Cari(s => s.Npm == npm.Trim()) != null)
            throw new ArgumentException("NPM sudah terdaftar");

        var siswa = new Siswa { Id = _nextSiswaId++, Nama = nama, Npm = npm };
        _siswa.Tambah(siswa);
        return siswa;
    }

    public List<Siswa> DaftarSiswa() => _siswa.Semua();

    public List<Siswa> CariSiswa(string kataKunci)
    {
        return _siswa.Semua()
            .Where(s => s.Nama.Contains(kataKunci, StringComparison.OrdinalIgnoreCase)
                     || s.Npm.Contains(kataKunci))
            .ToList();
    }

    public Pengeluaran BuatTagihan(string nama, decimal totalBiaya, List<int> idSiswaTerlibat, bool luarNegeri = false)
    {
        if (string.IsNullOrWhiteSpace(nama))
            throw new ArgumentException("Nama pengeluaran tidak boleh kosong");

        var ids = idSiswaTerlibat.Distinct().ToList();
        if (ids.Count == 0)
            throw new ArgumentException("Pilih minimal 1 siswa");

        foreach (var id in ids)
            if (_siswa.Cari(s => s.Id == id) == null)
                throw new ArgumentException($"Siswa dengan id {id} tidak ditemukan");

        Pengeluaran p = luarNegeri ? new PengeluaranLuarNegeri() : new PengeluaranBiasa();
        p.IdTagihan = _nextTagihanId++;
        p.NamaPengeluaran = nama;
        p.TotalBiaya = totalBiaya;
        p.IdSiswaTerlibat = ids;

        _tagihan.Tambah(p);
        foreach (var id in ids)
            _status[(p.IdTagihan, id)] = new StatusBayar { IdTagihan = p.IdTagihan, IdSiswa = id, IsLunas = false };

        return p;
    }

    public List<Pengeluaran> DaftarTagihan() => _tagihan.Semua();

    public void TandaiLunas(int idTagihan, int idSiswa)
    {
        if (!_status.TryGetValue((idTagihan, idSiswa), out var status))
            throw new ArgumentException("Siswa tidak termasuk dalam tagihan ini");
        status.IsLunas = true;
    }

    public (decimal terkumpul, decimal kurang) HitungDana(int idTagihan)
    {
        var t = AmbilTagihan(idTagihan);
        int jumlahLunas = _status.Values.Count(x => x.IdTagihan == idTagihan && x.IsLunas);

        decimal terkumpul = t.BiayaPerSiswa() * jumlahLunas;
        decimal kurang = t.HitungTotalAkhir() - terkumpul;
        return (terkumpul, kurang);
    }

    public string StatusTagihan(int idTagihan)
    {
        var t = AmbilTagihan(idTagihan);
        var sudah = new List<string>();
        var belum = new List<string>();

        foreach (var s in _status.Values.Where(x => x.IdTagihan == idTagihan))
        {
            var nama = _siswa.Cari(x => x.Id == s.IdSiswa)?.Nama ?? "(tidak dikenal)";
            if (s.IsLunas) sudah.Add(nama); else belum.Add(nama);
        }

        var (terkumpul, kurang) = HitungDana(idTagihan);
        return $"TAGIHAN: {t.NamaPengeluaran}\n" +
               $"Per orang: Rp {t.BiayaPerSiswa():N0}\n" +
               $"Sudah bayar: {(sudah.Count > 0 ? string.Join(", ", sudah) : "-")}\n" +
               $"Belum bayar: {(belum.Count > 0 ? string.Join(", ", belum) : "-")}\n" +
               $"Terkumpul: Rp {terkumpul:N0} | Kurang: Rp {kurang:N0}";
    }

    public List<StatusBayar> DaftarStatus() => _status.Values.ToList();

    private Pengeluaran AmbilTagihan(int idTagihan)
    {
        return _tagihan.Cari(x => x.IdTagihan == idTagihan)
               ?? throw new ArgumentException("Tagihan tidak ditemukan");
    }
}