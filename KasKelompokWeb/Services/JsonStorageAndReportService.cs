using System.Collections.Generic;
using System.IO;
using KasKelompok.Models;

namespace KasKelompok.Services
{
    /// <summary>
    /// Layanan Penyimpanan dan Pelaporan Kas Kelompok
    /// INHERITANCE: Mewarisi Generic Class JsonStorageService<KasDatabase>
    /// </summary>
    public class JsonStorageAndReportService : JsonStorageService<KasDatabase>
    {
        private readonly IWhatsAppService _waService;
        private readonly IExporter<Pengeluaran> _csvService;

        public JsonStorageAndReportService() : base(Path.Combine("App_Data", "patungan_kelas.json"))
        {
            _waService = new WhatsAppService();
            _csvService = new CsvExportService();
        }

        public string GenerateBroadcastWa(Pengeluaran p, List<Siswa> siswaList)
        {
            return _waService.GenerateBroadcastWa(p, siswaList);
        }

        public string GenerateLaporanCsv(List<Pengeluaran> list, List<Siswa> siswaList)
        {
            return _csvService.GenerateCsv(list, siswaList);
        }
    }
}
