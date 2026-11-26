using System.Text;
using System.Text.Json;
using KasKelompok.Models;

namespace KasKelompok.Services
{
    public class KasDatabase
    {
        public string NamaKelompok { get; set; } = "Kas Kelompok TI-3A";
        public decimal SaldoAwalKas { get; set; } = 350000;
        public string PinBendahara { get; set; } = "1234";
        public List<Siswa> SiswaList { get; set; } = new();
        public List<Pengeluaran> PengeluaranList { get; set; } = new();
        public List<AuditLogEntry> AuditLogs { get; set; } = new();
    }

    public class JsonStorageAndReportService
    {
        private readonly string _filePath = Path.Combine("App_Data", "patungan_kelas.json");
        private static readonly JsonSerializerOptions _jsonOpts = new() { WriteIndented = true };

        public KasDatabase Load()
        {
            if (!File.Exists(_filePath)) return new KasDatabase();
            string json = File.ReadAllText(_filePath);
            return JsonSerializer.Deserialize<KasDatabase>(json) ?? new KasDatabase();
        }

        public void Save(KasDatabase db)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
            File.WriteAllText(_filePath, JsonSerializer.Serialize(db, _jsonOpts));
        }

        public string GenerateBroadcastWa(Pengeluaran p, List<Siswa> siswaList)
        {
            var map = siswaList.ToDictionary(s => s.Id, s => s.Nama);
            decimal terkumpul = p.StatusBayar.Where(s => s.IsLunas).Sum(s => s.Porsi);
            decimal kurang = Math.Max(0, p.TotalBiaya - terkumpul);

            var sb = new StringBuilder();
            sb.AppendLine($"*INFO PATUNGAN: {p.NamaPengeluaran}*");
            sb.AppendLine($"Kategori: {p.Kategori} | Total: Rp {p.TotalBiaya:N0}");
            sb.AppendLine($"Terkumpul: Rp {terkumpul:N0} | Kekurangan: Rp {kurang:N0}");
            sb.AppendLine();
            sb.AppendLine("✅ *SUDAH LUNAS:*");
            foreach (var item in p.StatusBayar.Where(s => s.IsLunas))
                sb.AppendLine($"- {map.GetValueOrDefault(item.SiswaId, item.SiswaId)} (Rp {item.Porsi:N0})");

            sb.AppendLine();
            sb.AppendLine("❌ *BELUM BAYAR:*");
            foreach (var item in p.StatusBayar.Where(s => !s.IsLunas))
                sb.AppendLine($"- {map.GetValueOrDefault(item.SiswaId, item.SiswaId)} (Rp {item.Porsi:N0})");

            return sb.ToString();
        }

        public string GenerateLaporanCsv(List<Pengeluaran> list, List<Siswa> siswaList)
        {
            var map = siswaList.ToDictionary(s => s.Id, s => s);
            var sb = new StringBuilder();
            sb.AppendLine("IdTagihan,Tanggal,NamaPengeluaran,Kategori,NamaSiswa,NPM,Porsi,Status");

            foreach (var p in list)
            {
                foreach (var st in p.StatusBayar)
                {
                    var siswa = map.GetValueOrDefault(st.SiswaId);
                    string status = st.IsLunas ? "LUNAS" : "BELUM BAYAR";
                    sb.AppendLine($"{p.IdTagihan},{p.Tanggal:yyyy-MM-dd},\"{p.NamaPengeluaran}\",{p.Kategori},\"{siswa?.Nama}\",{siswa?.Npm},{st.Porsi},{status}");
                }
            }
            return sb.ToString();
        }
    }
}