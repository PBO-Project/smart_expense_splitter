using System.Collections.Generic;
using KasKelompok.Models;

namespace KasKelompok.Services
{
    /// <summary>
    /// Konsep PBO: Interface untuk layanan WhatsApp / Notifikasi
    /// </summary>
    public interface IWhatsAppService
    {
        string GenerateBroadcastWa(Pengeluaran p, List<Siswa> siswaList);
        Dictionary<string, decimal> HitungRekapStatus(Pengeluaran p);
        string GenerateWhatsAppUrl(string pesan, string nomorTujuan = "");
    }
}
