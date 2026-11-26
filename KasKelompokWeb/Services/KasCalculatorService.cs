using KasKelompok.Models;

namespace KasKelompok.Services
{
    public class KasCalculatorService
    {
        // Hitung pembagian rata maupun skala prioritas (anti pembagian nol & anti minus)
        public (decimal TotalBiaya, List<StatusBayar> Shares) HitungPorsiPatungan(
            decimal subtotal,
            decimal pajakPersen,
            List<(string SiswaId, decimal Bobot)> peserta,
            SplitMode mode)
        {
            if (subtotal <= 0)
                throw new ArgumentException("Nominal pengeluaran harus lebih dari 0.");
            if (pajakPersen < 0)
                throw new ArgumentException("Pajak/biaya admin tidak boleh negatif.");
            if (peserta == null || peserta.Count == 0)
                throw new InvalidOperationException("Minimal 1 siswa harus dipilih.");

            decimal totalBiaya = Math.Round(subtotal * (1 + pajakPersen / 100m), 0);
            var bobotList = peserta
                .Select(p => mode == SplitMode.Equal ? 1m : Math.Max(0.1m, p.Bobot))
                .ToList();

            decimal totalBobot = bobotList.Sum();
            var shares = new List<StatusBayar>();
            decimal allocated = 0;

            for (int i = 0; i < peserta.Count; i++)
            {
                decimal porsi = Math.Floor(totalBiaya * bobotList[i] / totalBobot);
                allocated += porsi;
                shares.Add(new StatusBayar
                {
                    SiswaId = peserta[i].SiswaId,
                    Bobot = bobotList[i],
                    Porsi = porsi,
                    IsLunas = false
                });
            }

            // Bagikan sisa pembulatan Rp 1 per orang agar jumlah porsi == totalBiaya
            int sisa = (int)(totalBiaya - allocated);
            for (int i = 0; i < sisa; i++)
            {
                shares[i % shares.Count].Porsi += 1;
            }

            return (totalBiaya, shares);
        }

        // Hitung total dana terkumpul & sisa kekurangan per pengeluaran
        public (decimal Terkumpul, decimal Kekurangan) HitungProgress(Pengeluaran p)
        {
            decimal terkumpul = p.StatusBayar.Where(s => s.IsLunas).Sum(s => s.Porsi);
            decimal kekurangan = Math.Max(0, p.TotalBiaya - terkumpul);
            return (terkumpul, kekurangan);
        }

        // Algoritma Penyederhanaan Utang (Greedy Min-Cash-Flow)
        public List<DebtTransfer> SederhanakanUtang(List<Pengeluaran> pengeluaranList, List<Siswa> siswaList)
        {
            var siswaMap = siswaList.ToDictionary(s => s.Id, s => s.Nama);
            var netBalance = siswaList.ToDictionary(s => s.Id, _ => 0m);

            foreach (var tagihan in pengeluaranList.Where(p => p.PaidById != "kas" && siswaMap.ContainsKey(p.PaidById)))
            {
                foreach (var sb in tagihan.StatusBayar.Where(s => !s.IsLunas && s.SiswaId != tagihan.PaidById))
                {
                    netBalance[sb.SiswaId] -= sb.Porsi;
                    netBalance[tagihan.PaidById] += sb.Porsi;
                }
            }

            var debtors = netBalance.Where(x => x.Value < 0)
                .Select(x => (Id: x.Key, Amount: Math.Abs(x.Value)))
                .OrderByDescending(x => x.Amount).ToList();

            var creditors = netBalance.Where(x => x.Value > 0)
                .Select(x => (Id: x.Key, Amount: x.Value))
                .OrderByDescending(x => x.Amount).ToList();

            var transfers = new List<DebtTransfer>();
            int i = 0, j = 0;

            while (i < debtors.Count && j < creditors.Count)
            {
                decimal nominal = Math.Min(debtors[i].Amount, creditors[j].Amount);
                if (nominal > 0)
                {
                    transfers.Add(new DebtTransfer
                    {
                        FromNama = siswaMap[debtors[i].Id],
                        ToNama = siswaMap[creditors[j].Id],
                        Amount = nominal
                    });
                }

                debtors[i] = (debtors[i].Id, debtors[i].Amount - nominal);
                creditors[j] = (creditors[j].Id, creditors[j].Amount - nominal);
                if (debtors[i].Amount == 0) i++;
                if (creditors[j].Amount == 0) j++;
            }

            return transfers;
        }
    }
}