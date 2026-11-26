using System.Text;
using Microsoft.AspNetCore.Mvc;
using KasKelompok.Models;
using KasKelompok.Services;

namespace KasKelompok.Controllers
{
    public record CreateTagihanRequest(
        string NamaPengeluaran,
        string Kategori,
        decimal Subtotal,
        decimal PajakPersen,
        SplitMode ModePatungan,
        string PaidById,
        List<PesertaInput> Peserta
    );

    public record PesertaInput(string SiswaId, decimal Bobot);

    [ApiController]
    [Route("api/[controller]")]
    public class KasController : ControllerBase
    {
        private readonly JsonStorageAndReportService _storage;
        private readonly KasCalculatorService _calc;

        public KasController(JsonStorageAndReportService storage, KasCalculatorService calc)
        {
            _storage = storage;
            _calc = calc;
        }

        // Cek Role Bendahara lewat header X-Bendahara-Pin (default: 1234)
        private bool IsBendahara(KasDatabase db)
        {
            string? pin = Request.Headers["X-Bendahara-Pin"].FirstOrDefault();
            return !string.IsNullOrWhiteSpace(pin) && pin == db.PinBendahara;
        }

        // READ-ONLY: Bisa diakses Anggota Kelas maupun Bendahara
        [HttpGet("ringkasan")]
        public IActionResult GetRingkasan([FromQuery] string? kategori)
        {
            var db = _storage.Load();
            var filtered = string.IsNullOrWhiteSpace(kategori)
                ? db.PengeluaranList
                : db.PengeluaranList.Where(p => p.Kategori.Equals(kategori, StringComparison.OrdinalIgnoreCase)).ToList();

            decimal totalKasTerpakai = db.PengeluaranList.Where(p => p.PaidById == "kas").Sum(p => p.TotalBiaya);
            decimal totalKasTerkumpul = db.PengeluaranList.Where(p => p.PaidById == "kas")
                .SelectMany(p => p.StatusBayar).Where(s => s.IsLunas).Sum(s => s.Porsi);

            return Ok(new
            {
                db.NamaKelompok,
                SisaSaldoKas = db.SaldoAwalKas + totalKasTerkumpul - totalKasTerpakai,
                Tagihan = filtered,
                PenyederhanaanUtang = _calc.SederhanakanUtang(db.PengeluaranList, db.SiswaList),
                db.AuditLogs
            });
        }

        // ADMIN ONLY: Buat tagihan patungan baru
        [HttpPost("tagihan")]
        public IActionResult BuatTagihan([FromBody] CreateTagihanRequest req)
        {
            var db = _storage.Load();
            if (!IsBendahara(db))
                return StatusCode(403, "Akses ditolak. Hanya Bendahara yang dapat membuat tagihan.");

            try
            {
                var peserta = req.Peserta.Select(p => (p.SiswaId, p.Bobot)).ToList();
                var (totalBiaya, shares) = _calc.HitungPorsiPatungan(req.Subtotal, req.PajakPersen, peserta, req.ModePatungan);

                var tagihanBaru = new Pengeluaran
                {
                    IdTagihan = $"TGH-{db.PengeluaranList.Count + 1:000}",
                    NamaPengeluaran = req.NamaPengeluaran,
                    Kategori = req.Kategori,
                    Tanggal = DateTime.Today,
                    Subtotal = req.Subtotal,
                    PajakPersen = req.PajakPersen,
                    TotalBiaya = totalBiaya,
                    ModePatungan = req.ModePatungan,
                    PaidById = string.IsNullOrWhiteSpace(req.PaidById) ? "kas" : req.PaidById,
                    StatusBayar = shares
                };

                db.PengeluaranList.Insert(0, tagihanBaru);
                db.AuditLogs.Insert(0, new AuditLogEntry
                {
                    Action = "Tambah Tagihan",
                    Detail = $"Membuat {tagihanBaru.NamaPengeluaran} senilai Rp {totalBiaya:N0}"
                });
                _storage.Save(db);

                return Ok(tagihanBaru);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // ADMIN ONLY: Centang / Batal Lunas (Mark as Paid)
        [HttpPost("tagihan/{idTagihan}/lunas/{siswaId}")]
        public IActionResult ToggleLunas(string idTagihan, string siswaId)
        {
            var db = _storage.Load();
            if (!IsBendahara(db))
                return StatusCode(403, "Akses ditolak. Hanya Bendahara yang berhak mencentang lunas.");

            var tagihan = db.PengeluaranList.FirstOrDefault(x => x.IdTagihan == idTagihan);
            var status = tagihan?.StatusBayar.FirstOrDefault(s => s.SiswaId == siswaId);
            if (tagihan == null || status == null) return NotFound();

            status.IsLunas = !status.IsLunas;
            status.PaidAt = status.IsLunas ? DateTime.Now : null;

            db.AuditLogs.Insert(0, new AuditLogEntry
            {
                Action = status.IsLunas ? "Centang Lunas" : "Batal Lunas",
                Detail = $"Siswa {siswaId} pada {tagihan.NamaPengeluaran} (Rp {status.Porsi:N0})"
            });
            _storage.Save(db);

            return Ok(tagihan);
        }

        // Generator Teks WA per tagihan
        [HttpGet("tagihan/{idTagihan}/wa")]
        public IActionResult GetTeksWa(string idTagihan)
        {
            var db = _storage.Load();
            var tagihan = db.PengeluaranList.FirstOrDefault(x => x.IdTagihan == idTagihan);
            if (tagihan == null) return NotFound();

            return Content(_storage.GenerateBroadcastWa(tagihan, db.SiswaList), "text/plain", Encoding.UTF8);
        }

        // Ekspor CSV Laporan Kas
        [HttpGet("laporan/csv")]
        public IActionResult ExportCsv()
        {
            var db = _storage.Load();
            string csv = _storage.GenerateLaporanCsv(db.PengeluaranList, db.SiswaList);
            return File(Encoding.UTF8.GetBytes(csv), "text/csv", "patungan_kelas_laporan.csv");
        }

    }
}