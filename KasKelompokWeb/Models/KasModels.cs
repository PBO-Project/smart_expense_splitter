using System;
using System.Collections.Generic;
using System.Linq;

namespace KasKelompok.Models
{
    public enum SplitMode { Equal, Weighted }
    public enum UserRole { Bendahara, Anggota }

    // =========================================================================
    // POIN 1: VARIABLE, GET, SET, METHOD (Class Induk Siswa - Enkapsulasi)
    // =========================================================================
    public class Siswa
    {
        private string _id = string.Empty;
        private string _nama = string.Empty;
        private string _npm = string.Empty;

        public string Id
        {
            get => _id;
            set => _id = value;
        }

        public string Nama
        {
            get => _nama;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Nama siswa tidak boleh kosong.");
                _nama = value.Trim();
            }
        }

        public string Npm
        {
            get => _npm;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("NPM/NIM wajib diisi.");
                _npm = value.Trim();
            }
        }

        public string NomorHp { get; set; } = string.Empty;

        // Constructor kosong untuk System.Text.Json
        public Siswa() { }

        public Siswa(string id, string nama, string npm, string nomorHp = "")
        {
            Id = id;
            Nama = nama;
            Npm = npm;
            NomorHp = nomorHp;
        }

        // Method Getter & Setter eksplisit
        public string GetNama() => _nama;
        public void SetNama(string namaBaru) => Nama = namaBaru;

        // Method virtual untuk di-override di class turunan
        public virtual string GetInfoAkses()
        {
            return $"{Nama} ({Npm}) - Anggota Kelas (Hanya Lihat)";
        }
    }

    // =========================================================================
    // POIN 2: INHERITANCE (Bendahara mewarisi Siswa)
    // =========================================================================
    public class Bendahara : Siswa
    {
        private string _pinAkses = "1234";

        public string PinAkses
        {
            get => _pinAkses;
            set
            {
                if (value.Length < 4)
                    throw new ArgumentException("PIN minimal 4 digit.");
                _pinAkses = value;
            }
        }

        public Bendahara() { }

        public Bendahara(string id, string nama, string npm, string pinAkses, string nomorHp = "")
            : base(id, nama, npm, nomorHp)
        {
            PinAkses = pinAkses;
        }

        public bool ValidasiPin(string inputPin) => inputPin == _pinAkses;

        public override string GetInfoAkses()
        {
            return $"{Nama} ({Npm}) - Bendahara (Admin Penuh)";
        }
    }

    // =========================================================================
    // POIN 1 & 2 PADA TRANSAKSI (TransaksiDasar -> Pengeluaran)
    // =========================================================================
    public class TransaksiDasar
    {
        private string _namaPengeluaran = string.Empty;
        private decimal _totalBiaya;

        public string IdTagihan { get; set; } = string.Empty;
        public DateTime Tanggal { get; set; } = DateTime.Today;

        public string NamaPengeluaran
        {
            get => _namaPengeluaran;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Nama pengeluaran tidak boleh kosong.");
                _namaPengeluaran = value.Trim();
            }
        }

        public decimal TotalBiaya
        {
            get => _totalBiaya;
            set
            {
                if (value < 0)
                    throw new ArgumentException("Total biaya tidak boleh negatif.");
                _totalBiaya = value;
            }
        }

        public decimal GetTotalBiaya() => _totalBiaya;
        public void SetTotalBiaya(decimal nominal) => TotalBiaya = nominal;

        public virtual decimal HitungTotalAkhir() => TotalBiaya;

        public virtual string CetakInfo()
        {
            return $"[{IdTagihan}] {NamaPengeluaran} - Rp {TotalBiaya:N0}";
        }
    }

    // INHERITANCE: Pengeluaran mewarisi TransaksiDasar
    public class Pengeluaran : TransaksiDasar
    {
        public string Kategori { get; set; } = "Lainnya";
        public decimal Subtotal { get; set; }
        public decimal PajakPersen { get; set; }
        public SplitMode ModePatungan { get; set; } = SplitMode.Equal;
        public string PaidById { get; set; } = "kas";
        public List<StatusBayar> StatusBayar { get; set; } = new();

        public decimal HitungTerkumpul()
        {
            return StatusBayar.Where(s => s.IsLunas).Sum(s => s.Porsi);
        }

        public override decimal HitungTotalAkhir()
        {
            return Math.Round(Subtotal * (1 + PajakPersen / 100m), 0);
        }

        public override string CetakInfo()
        {
            return $"[{IdTagihan}] {NamaPengeluaran} ({Kategori}) - Total: Rp {TotalBiaya:N0} | Terkumpul: Rp {HitungTerkumpul():N0}";
        }
    }

    // INHERITANCE: Pengeluaran Luar Negeri (Pajak 11%)
    public class PengeluaranLuarNegeri : Pengeluaran
    {
        public PengeluaranLuarNegeri()
        {
            Kategori = "Luar Negeri";
            PajakPersen = 11m; // PPN Luar Negeri 11%
        }
    }

    // =========================================================================
    // STATUS BAYAR, TRANSFER UTANG, & AUDIT LOG
    // =========================================================================
    public class StatusBayar
    {
        public string SiswaId { get; set; } = string.Empty;
        public decimal Bobot { get; set; } = 1m;
        public decimal Porsi { get; set; }
        public bool IsLunas { get; set; }
        public DateTime? PaidAt { get; set; }
    }

    public class DebtTransfer
    {
        public string FromNama { get; set; } = string.Empty;
        public string ToNama { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }

    public class AuditLogEntry
    {
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public string Actor { get; set; } = "Bendahara";
        public string Action { get; set; } = string.Empty;
        public string Detail { get; set; } = string.Empty;
    }

    public class KasDatabase
    {
        public string NamaKelompok { get; set; } = "Kas Kelompok TI-3A";
        public decimal SaldoAwalKas { get; set; } = 350000;
        public string PinBendahara { get; set; } = "1234";
        public List<Siswa> SiswaList { get; set; } = new();
        public List<Pengeluaran> PengeluaranList { get; set; } = new();
        public List<AuditLogEntry> AuditLogs { get; set; } = new();
    }
}
