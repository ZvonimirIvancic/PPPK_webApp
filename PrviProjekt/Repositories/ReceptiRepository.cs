using Microsoft.EntityFrameworkCore;
using PrviProjekt.Data;
using PrviProjekt.Models;

namespace PrviProjekt.Repositories
{
    public class ReceptiRepository : Repository<Recept>, IReceptiRepository
    {
        public ReceptiRepository(MedicinskiDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Recept>> GetByPacijentIdAsync(int pacijentId)
        {
            return await _dbSet
                .Where(r => r.PacijentId == pacijentId)
                .Include(r => r.Pacijent)
                .Include(r => r.Pregled)
                .OrderByDescending(r => r.DatumIzdavanja)
                .ToListAsync();
        }

        public async Task<IEnumerable<Recept>> GetByPregledIdAsync(int pregledId)
        {
            return await _dbSet
                .Where(r => r.PregledId == pregledId)
                .Include(r => r.Pacijent)
                .Include(r => r.Pregled)
                .OrderByDescending(r => r.DatumIzdavanja)
                .ToListAsync();
        }

        public async Task<IEnumerable<Recept>> GetValidReceptiAsync()
        {
            var today = DateTime.Today;
            return await _dbSet
                .Where(r => !r.DatumVazenja.HasValue || r.DatumVazenja >= today)
                .Include(r => r.Pacijent)
                .Include(r => r.Pregled)
                .OrderByDescending(r => r.DatumIzdavanja)
                .ToListAsync();
        }

        public async Task<IEnumerable<Recept>> GetExpiringReceptiAsync(int daysAhead = 30)
        {
            var today = DateTime.Today;
            var endDate = today.AddDays(daysAhead);

            return await _dbSet
                .Where(r => r.DatumVazenja.HasValue &&
                           r.DatumVazenja >= today &&
                           r.DatumVazenja <= endDate)
                .Include(r => r.Pacijent)
                .Include(r => r.Pregled)
                .OrderBy(r => r.DatumVazenja)
                .ToListAsync();
        }

        public async Task<IEnumerable<Recept>> GetExpiredReceptiAsync()
        {
            var today = DateTime.Today;
            return await _dbSet
                .Where(r => r.DatumVazenja.HasValue && r.DatumVazenja < today)
                .Include(r => r.Pacijent)
                .Include(r => r.Pregled)
                .OrderByDescending(r => r.DatumVazenja)
                .ToListAsync();
        }

        public async Task<IEnumerable<Recept>> GetByLijekAsync(string nazivLijeka)
        {
            return await _dbSet
                .Where(r => r.NazivLijeka.ToLower().Contains(nazivLijeka.ToLower()))
                .Include(r => r.Pacijent)
                .Include(r => r.Pregled)
                .OrderByDescending(r => r.DatumIzdavanja)
                .ToListAsync();
        }

        public async Task<IEnumerable<Recept>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate)
        {
            return await _dbSet
                .Where(r => r.DatumIzdavanja >= fromDate.Date && r.DatumIzdavanja <= toDate.Date)
                .Include(r => r.Pacijent)
                .Include(r => r.Pregled)
                .OrderByDescending(r => r.DatumIzdavanja)
                .ToListAsync();
        }

        public async Task<Dictionary<string, int>> GetLijekStatisticsAsync()
        {
            return await _dbSet
                .GroupBy(r => r.NazivLijeka)
                .ToDictionaryAsync(g => g.Key, g => g.Count());
        }

        public async Task<int> GetActiveReceptiCountAsync()
        {
            var today = DateTime.Today;
            return await _dbSet
                .CountAsync(r => !r.DatumVazenja.HasValue || r.DatumVazenja >= today);
        }

        public override async Task<Recept?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(r => r.Pacijent)
                .Include(r => r.Pregled)
                .FirstOrDefaultAsync(r => r.Id == id);
        }
    }
}

