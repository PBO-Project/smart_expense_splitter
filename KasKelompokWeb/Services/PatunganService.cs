using System;
using System.Collections.Generic;
using System.Linq;
using KasKelompok.Models;

namespace KasKelompok.Services
{
    /// <summary>
    /// Service Logika Patungan
    /// Menggunakan Generic IRepository<T>, Collection List, dan Dictionary
    /// </summary>
    public class PatunganService : IPatunganService
    {
        private readonly IRepository<Siswa> _siswa;
        private readonly IRepository<Pengeluaran> _tagihan;

        // Dictionary: key = (idTagihan, idSiswa)
        private readonly Dictionary<(string idTagihan, string idSiswa), StatusBayar> _status = new();

        private int _counterSiswa = 1;
        private int _counterTagihan = 1;

        public PatunganService(IRepository<Siswa> siswaRepo, IRepository<Pengeluaran> tagihanRepo)
        {
            _siswa = siswaRepo;
            _tagihan = tagihanRepo;
        }

        public PatunganService() : this(new Repository<Siswa>(), new Repository<Pengeluaran>())
        {
        }

        public Siswa TambahSiswa(string nama, string npm, string nomorHp = "")
        {
            if (_siswa.Cari(s => s.Npm == npm.Trim()) != null)
                throw new ArgumentException("NPM sudah terdaftar");

            string id = $"S{_counterSiswa++:00}";
            var siswa = new Siswa(id, nama, npm, nomorHp);
            _siswa.Tambah(siswa);
            return siswa;
        }

        public List<Siswa> DaftarSiswa() => _siswa.Semua();

        public List<Siswa> CariSiswa(string kataKunci)
        {
            return _siswa.Semua()
                .Where(s => s.Nama.Contains(kataKunci, StringComparison.OrdinalIgnoreCase)
                         || s.Npm.Contains(kataKunci, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        public Pengeluaran BuatTagihan(string nama, decimal totalBiaya, List<string> idSiswaTerlibat, bool luarNegeri = false)
        {
            if (string.IsNullOrWhiteSpace(nama))
                throw new ArgumentException("Nama pengeluaran tidak boleh kosong");

            var ids = idSiswaTerlibat.Distinct().ToList();
            if (ids.Count == 0)
                throw new ArgumentException("Pilih minimal 1 siswa");

            foreach (var id in ids)
            {
                if (_siswa.Cari(s => s.Id == id) == null)
                    throw new ArgumentException($"Siswa dengan id {id} tidak ditemukan");
            }

            Pengeluaran p = luarNegeri ? new PengeluaranLuarNegeri() : new Pengeluaran();
            p.IdTagihan = $"TGH-{_counterTagihan++:000}";
            p.NamaPengeluaran = nama;
            p.Subtotal = totalBiaya;
            p.TotalBiaya = p.HitungTotalAkhir();

            decimal porsiPerOrang = Math.Round(p.TotalBiaya / ids.Count, 0);

            foreach (var id in ids)
            {
                var sb = new StatusBayar
                {
                    SiswaId = id,
                    Porsi = porsiPerOrang,
                    IsLunas = false
                };
                p.StatusBayar.Add(sb);
                _status[(p.IdTagihan, id)] = sb;
            }

            _tagihan.Tambah(p);
            return p;
        }

        public List<Pengeluaran> DaftarTagihan() => _tagihan.Semua();

        public void TandaiLunas(string idTagihan, string idSiswa)
        {
            if (!_status.TryGetValue((idTagihan, idSiswa), out var status))
                throw new ArgumentException("Siswa tidak termasuk dalam tagihan ini");
            status.IsLunas = true;
            status.PaidAt = DateTime.Now;
        }

        public (decimal terkumpul, decimal kurang) HitungDana(string idTagihan)
        {
            var t = AmbilTagihan(idTagihan);
            decimal terkumpul = t.StatusBayar.Where(x => x.IsLunas).Sum(x => x.Porsi);
            decimal kurang = Math.Max(0, t.TotalBiaya - terkumpul);
            return (terkumpul, kurang);
        }

        public string StatusTagihan(string idTagihan)
        {
            var t = AmbilTagihan(idTagihan);
            var sudah = new List<string>();
            var belum = new List<string>();

            foreach (var s in t.StatusBayar)
            {
                var nama = _siswa.Cari(x => x.Id == s.SiswaId)?.Nama ?? s.SiswaId;
                if (s.IsLunas) sudah.Add(nama); else belum.Add(nama);
            }

            var (terkumpul, kurang) = HitungDana(idTagihan);
            return $"TAGIHAN: {t.NamaPengeluaran}\n" +
                   $"Sudah bayar: {(sudah.Count > 0 ? string.Join(", ", sudah) : "-")}\n" +
                   $"Belum bayar: {(belum.Count > 0 ? string.Join(", ", belum) : "-")}\n" +
                   $"Terkumpul: Rp {terkumpul:N0} | Kurang: Rp {kurang:N0}";
        }

        public List<StatusBayar> DaftarStatus() => _status.Values.ToList();

        private Pengeluaran AmbilTagihan(string idTagihan)
        {
            return _tagihan.Cari(x => x.IdTagihan == idTagihan)
                   ?? throw new ArgumentException("Tagihan tidak ditemukan");
        }
    }
}
