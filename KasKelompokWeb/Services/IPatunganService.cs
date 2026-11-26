using KasKelompokWeb.Models;

namespace KasKelompokWeb.Services;

public interface IPatunganService
{
    Siswa TambahSiswa(string nama, string npm);
    List<Siswa> DaftarSiswa();
    List<Siswa> CariSiswa(string kataKunci);

    Pengeluaran BuatTagihan(string nama, decimal totalBiaya, List<int> idSiswaTerlibat, bool luarNegeri = false);
    List<Pengeluaran> DaftarTagihan();

    void TandaiLunas(int idTagihan, int idSiswa);
    (decimal terkumpul, decimal kurang) HitungDana(int idTagihan);
    string StatusTagihan(int idTagihan);

    List<StatusBayar> DaftarStatus();
}