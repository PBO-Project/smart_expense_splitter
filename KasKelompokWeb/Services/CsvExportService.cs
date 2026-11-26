using System.Collections.Generic;
using System.Linq;
using System.Text;
using KasKelompok.Models;

namespace KasKelompok.Services
{
    /// <summary>
    /// Konsep PBO:
    /// - Mengimplementasikan Generic Interface IExporter<Pengeluaran>
    /// - Collection (IEnumerable, List)
    /// - Dictionary untuk mapping cepat SiswaId ke Siswa
    /// </summary>
    public class CsvExportService : IExporter<Pengeluaran>
    {
        public string GenerateCsv(IEnumerable<Pengeluaran> list, List<Siswa> siswaList)
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
