using System.Text.Json.Serialization;

namespace KasKelompokWeb.Models;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "tipe")]
[JsonDerivedType(typeof(PengeluaranBiasa), "biasa")]
[JsonDerivedType(typeof(PengeluaranLuarNegeri), "luarnegeri")]
public abstract class Pengeluaran
{
    private decimal _totalBiaya;

    public int IdTagihan { get; set; }
    public string NamaPengeluaran { get; set; } = "";
    public DateTime Tanggal { get; set; } = DateTime.Now;
    public List<int> IdSiswaTerlibat { get; set; } = new();

    public decimal TotalBiaya
    {
        get => _totalBiaya;
        set
        {
            if (value <= 0)
                throw new ArgumentException("Total biaya harus lebih dari 0");
            _totalBiaya = value;
        }
    }

    public abstract decimal HitungTotalAkhir();

    public decimal BiayaPerSiswa()
    {
        if (IdSiswaTerlibat.Count == 0)
            throw new InvalidOperationException("Tidak ada siswa yang terlibat");
        return HitungTotalAkhir() / IdSiswaTerlibat.Count;
    }
}

public class PengeluaranBiasa : Pengeluaran
{
    public override decimal HitungTotalAkhir() => TotalBiaya;
}

public class PengeluaranLuarNegeri : Pengeluaran
{
    public decimal Pajak { get; set; } = 0.11m;

    public override decimal HitungTotalAkhir() => TotalBiaya + (TotalBiaya * Pajak);
}