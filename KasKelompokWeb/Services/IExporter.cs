using System.Collections.Generic;

namespace KasKelompok.Services
{
    /// <summary>
    /// Konsep PBO: Generic Interface untuk Ekspor
    /// </summary>
    public interface IExporter<T>
    {
        string GenerateCsv(IEnumerable<T> data, List<KasKelompok.Models.Siswa> siswaList);
    }
}
