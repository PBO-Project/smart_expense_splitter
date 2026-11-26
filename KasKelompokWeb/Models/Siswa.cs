namespace KasKelompokWeb.Models;

public class Siswa
{
    private string _nama = "";
    private string _npm = "";

    public int Id { get; set; }

    public string Nama
    {
        get => _nama;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Nama tidak boleh kosong");
            _nama = value.Trim();
        }
    }

    public string Npm
    {
        get => _npm;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("NPM tidak boleh kosong");
            _npm = value.Trim();
        }
    }

    public string Info() => $"{Nama} ({Npm})";
}