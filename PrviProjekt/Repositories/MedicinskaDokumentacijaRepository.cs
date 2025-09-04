using Microsoft.EntityFrameworkCore;
using PrviProjekt.Data;
using PrviProjekt.Models;

namespace PrviProjekt.Repositories
{
    public class MedicinskaDokumentacijaRepository : Repository<MedicinskaDokumentacija>, IMedicinskaDokumentacijaRepository
    {
        public MedicinskaDokumentacijaRepository(MedicinskiDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<MedicinskaDokumentacija>> GetByPacijentIdAsync(int pacijentId)
        {
            return await _dbSet
                .Where(md => md.PacijentId == pacijentId)
                .Include(md => md.Pacijent)
                .OrderByDescending(md => md.DatumPocetka)
                .ToListAsync();
        }

        public async Task<IEnumerable<MedicinskaDokumentacija>> GetActiveBoljestiAsync(int pacijentId)
        {
            return await _dbSet
                .Where(md => md.PacijentId == pacijentId && !md.DatumZavrsetka.HasValue)
                .Include(md => md.Pacijent)
                .OrderByDescending(md => md.DatumPocetka)
                .ToListAsync();
        }

        public async Task<IEnumerable<MedicinskaDokumentacija>> GetByBolestAsync(string nazivBolesti)
        {
            return await _dbSet
                .Where(md => md.NazivBolesti.ToLower().Contains(nazivBolesti.ToLower()))
                .Include(md => md.Pacijent)
                .OrderByDescending(md => md.DatumPocetka)
                .ToListAsync();
        }

        public async Task<IEnumerable<MedicinskaDokumentacija>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate)
        {
            return await _dbSet
                .Where(md => md.DatumPocetka >= fromDate.Date && md.DatumPocetka <= toDate.Date)
                .Include(md => md.Pacijent)
                .OrderByDescending(md => md.DatumPocetka)
                .ToListAsync();
        }

        public async Task<IEnumerable<MedicinskaDokumentacija>> GetLongestActiveBoljestiAsync(int count = 10)
        {
            var today = DateTime.Today;
            return await _dbSet
                .Where(md => !md.DatumZavrsetka.HasValue)
                .Include(md => md.Pacijent)
                .OrderBy(md => md.DatumPocetka)
                .Take(count)
                .ToListAsync();
        }

        public async Task<Dictionary<string, int>> GetBolestStatisticsAsync()
        {
            return await _dbSet
                .GroupBy(md => md.NazivBolesti)
                .ToDictionaryAsync(g => g.Key, g => g.Count());
        }

        public async Task<Dictionary<string, double>> GetAverageTrajanjeByBolestAsync()
        {
            var completedBolesti = await _dbSet
                .Where(md => md.DatumZavrsetka.HasValue)
                .ToListAsync();

            return completedBolesti
                .GroupBy(md => md.NazivBolesti)
                .ToDictionary(
                    g => g.Key,
                    g => g.Average(md => (md.DatumZavrsetka!.Value - md.DatumPocetka).TotalDays)
                );
        }

        public async Task<IEnumerable<MedicinskaDokumentacija>> GetRecentlyCompletedAsync(int days = 30)
        {
            var cutoffDate = DateTime.Today.AddDays(-days);
            return await _dbSet
                .Where(md => md.DatumZavrsetka.HasValue && md.DatumZavrsetka >= cutoffDate)
                .Include(md => md.Pacijent)
                .OrderByDescending(md => md.DatumZavrsetka)
                .ToListAsync();
        }

        public override async Task<MedicinskaDokumentacija?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(md => md.Pacijent)
                .FirstOrDefaultAsync(md => md.Id == id);
        }
    }
}
