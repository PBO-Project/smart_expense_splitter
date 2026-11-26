using KasKelompok.Models;
using KasKelompok.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Daftarkan Controller & Service
builder.Services.AddControllers();
builder.Services.AddSingleton<KasCalculatorService>();
builder.Services.AddSingleton<JsonStorageAndReportService>();

var app = builder.Build();

// 2. Auto-seed data awal ke App_Data/patungan_kelas.json jika file masih kosong
using (var scope = app.Services.CreateScope())
{
    var storage = scope.ServiceProvider.GetRequiredService<JsonStorageAndReportService>();
    var calc = scope.ServiceProvider.GetRequiredService<KasCalculatorService>();
    var db = storage.Load();

    if (db.SiswaList.Count == 0)
    {
        db.SiswaList.AddRange(new List<Siswa>
        {
            new Bendahara("S01", "Raka Pratama", "2310511001", "1234"),
            new Siswa("S02", "Nadia Kusuma", "2310511014"),
            new Siswa("S03", "Dimas Wijaya", "2310511022"),
            new Siswa("S04", "Salsa Aurelia", "2310511035")
        });

        var peserta = db.SiswaList.Select(s => (s.Id, 1m)).ToList();
        var (totalBiaya, shares) = calc.HitungPorsiPatungan(300000, 0, peserta, SplitMode.Equal);
        shares[0].IsLunas = true;
        shares[0].PaidAt = DateTime.Now;

        db.PengeluaranList.Add(new Pengeluaran
        {
            IdTagihan = "TGH-001",
            NamaPengeluaran = "Beli Hosting & Domain Proyek",
            Kategori = "Hosting & Tools",
            Subtotal = 300000,
            PajakPersen = 0,
            TotalBiaya = totalBiaya,
            ModePatungan = SplitMode.Equal,
            PaidById = "kas",
            StatusBayar = shares
        });

        db.AuditLogs.Add(new AuditLogEntry
        {
            Action = "Inisialisasi Data",
            Detail = "Data awal kas kelompok berhasil dibuat otomatis."
        });

        storage.Save(db);
    }
}

// 3. Jalankan middleware standar ASP.NET Core
app.UseHttpsRedirection();
app.UseDefaultFiles();
app.UseStaticFiles();
app.MapControllers();

app.Run();