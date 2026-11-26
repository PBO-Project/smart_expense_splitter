using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KasKelompok.Models;

namespace KasKelompok.Services
{
    /// <summary>
    /// Konsep PBO:
    /// - Abstract Class untuk Formatter Pesan
    /// </summary>
    public abstract class BaseMessageFormatter
    {
        public string HeaderIcon { get; set; } = "📢";
        public abstract string BuatHeader(string judul);
        public abstract string BuatFooter();
    }

    /// <summary>
    /// Konsep PBO yang diimplementasikan:
    /// 1. Inheritance: Mewarisi BaseMessageFormatter
    /// 2. Interface: Mengimplementasikan IWhatsAppService
    /// 3. Collection: Mengolah List<Siswa> dan List<StatusBayar>
    /// 4. Dictionary: Menggunakan Dictionary<string, decimal> untuk rekapitulasi data
    /// </summary>
    public class WhatsAppService : BaseMessageFormatter, IWhatsAppService
    {
        public override string BuatHeader(string judul)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"{HeaderIcon} *INFO PATUNGAN KAS KELAS* {HeaderIcon}");
            sb.AppendLine($"📌 *Kegiatan:* {judul}");
            sb.AppendLine($"🗓️ *Tanggal Cetak:* {DateTime.Now:dd MMMM yyyy}");
            sb.AppendLine("----------------------------------------");
            return sb.ToString();
        }

        public override string BuatFooter()
        {
            var sb = new StringBuilder();
            sb.AppendLine("💳 *Metode Pembayaran:* Transfer Bank / E-Wallet ke Bendahara");
            sb.AppendLine("🙏 _Dimohon untuk segera melunasi tepat waktu. Terima kasih banyak!_");
            return sb.ToString();
        }

        /// <summary>
        /// Penggunaan Konsep Collection (List) dan Dictionary
        /// </summary>
        public Dictionary<string, decimal> HitungRekapStatus(Pengeluaran p)
        {
            decimal terkumpul = p.StatusBayar.Where(s => s.IsLunas).Sum(s => s.Porsi);
            decimal kekurangan = Math.Max(0, p.TotalBiaya - terkumpul);

            return new Dictionary<string, decimal>
            {
                { "TotalBiaya", p.TotalBiaya },
                { "TotalTerkumpul", terkumpul },
                { "Kekurangan", kekurangan },
                { "JumlahPeserta", p.StatusBayar.Count },
                { "JumlahLunas", p.StatusBayar.Count(s => s.IsLunas) }
            };
        }

        public string GenerateBroadcastWa(Pengeluaran p, List<Siswa> siswaList)
        {
            var map = siswaList.ToDictionary(s => s.Id, s => s.Nama);
            var rekap = HitungRekapStatus(p);

            var sb = new StringBuilder();
            sb.Append(BuatHeader(p.NamaPengeluaran));
            sb.AppendLine($"Kategori: {p.Kategori} | Total: Rp {rekap["TotalBiaya"]:N0}");
            sb.AppendLine($"Terkumpul: Rp {rekap["TotalTerkumpul"]:N0} | Kekurangan: Rp {rekap["Kekurangan"]:N0}");
            sb.AppendLine();

            sb.AppendLine($"✅ *SUDAH LUNAS ({rekap["JumlahLunas"]}/{rekap["JumlahPeserta"]}):*");
            var lunas = p.StatusBayar.Where(s => s.IsLunas).ToList();
            if (lunas.Count > 0)
            {
                foreach (var item in lunas)
                {
                    string nama = map.GetValueOrDefault(item.SiswaId, item.SiswaId);
                    sb.AppendLine($"- {nama} (Rp {item.Porsi:N0})");
                }
            }
            else
            {
                sb.AppendLine("  _(Belum ada yang melunasi)_");
            }

            sb.AppendLine();
            sb.AppendLine($"❌ *BELUM BAYAR ({rekap["JumlahPeserta"] - rekap["JumlahLunas"]}/{rekap["JumlahPeserta"]}):*");
            var belumLunas = p.StatusBayar.Where(s => !s.IsLunas).ToList();
            if (belumLunas.Count > 0)
            {
                foreach (var item in belumLunas)
                {
                    string nama = map.GetValueOrDefault(item.SiswaId, item.SiswaId);
                    sb.AppendLine($"- {nama} (Rp {item.Porsi:N0})");
                }
            }
            else
            {
                sb.AppendLine("  🎉 _Semua sudah lunas!_");
            }

            sb.AppendLine("----------------------------------------");
            sb.Append(BuatFooter());

            return sb.ToString();
        }

        public string GenerateWhatsAppUrl(string pesan, string nomorTujuan = "")
        {
            string encoded = Uri.EscapeDataString(pesan);
            if (!string.IsNullOrWhiteSpace(nomorTujuan))
            {
                string cleanPhone = new string(nomorTujuan.Where(char.IsDigit).ToArray());
                if (cleanPhone.StartsWith("0"))
                {
                    cleanPhone = "62" + cleanPhone.Substring(1);
                }
                return $"https://api.whatsapp.com/send?phone={cleanPhone}&text={encoded}";
            }
            return $"https://api.whatsapp.com/send?text={encoded}";
        }
    }
}
