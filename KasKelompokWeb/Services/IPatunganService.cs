using System.Collections.Generic;
using KasKelompok.Models;

namespace KasKelompok.Services
{
    public interface IPatunganService
    {
        Siswa TambahSiswa(string nama, string npm, string nomorHp = "");
        List<Siswa> DaftarSiswa();
        List<Siswa> CariSiswa(string kataKunci);

        Pengeluaran BuatTagihan(string nama, decimal totalBiaya, List<string> idSiswaTerlibat, bool luarNegeri = false);
        List<Pengeluaran> DaftarTagihan();

        void TandaiLunas(string idTagihan, string idSiswa);
        (decimal terkumpul, decimal kurang) HitungDana(string idTagihan);
        string StatusTagihan(string idTagihan);

        List<StatusBayar> DaftarStatus();
    }
}
