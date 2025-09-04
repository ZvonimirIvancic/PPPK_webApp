using Microsoft.EntityFrameworkCore;
using PrviProjekt.Data;
using PrviProjekt.Enums;
using PrviProjekt.Models;

namespace PrviProjekt.Repositories
{
    public class PreglediRepository : Repository<Pregled>, IPreglediRepository
    {
        public PreglediRepository(MedicinskiDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Pregled>> GetByPacijentIdAsync(int pacijentId)
        {
            return await _dbSet
                .Where(p => p.PacijentId == pacijentId)
                .Include(p => p.Pacijent)
                .Include(p => p.Slike)
                .OrderByDescending(p => p.DatumPregleda)
                .ThenByDescending(p => p.VrijemePregleda)
                .ToListAsync();
        }

        public async Task<IEnumerable<Pregled>> GetByTipPregledaAsync(TipPregleda tipPregleda)
        {
            return await _dbSet
                .Where(p => p.TipPregleda == tipPregleda)
                .Include(p => p.Pacijent)
                .OrderByDescending(p => p.DatumPregleda)
                .ToListAsync();
        }

        public async Task<IEnumerable<Pregled>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate)
        {
            return await _dbSet
                .Where(p => p.DatumPregleda >= fromDate.Date && p.DatumPregleda <= toDate.Date)
                .Include(p => p.Pacijent)
                .OrderBy(p => p.DatumPregleda)
                .ThenBy(p => p.VrijemePregleda)
                .ToListAsync();
        }

        public async Task<Pregled?> GetWithSlikeAsync(int id)
        {
            return await _dbSet
                .Include(p => p.Pacijent)
                .Include(p => p.Slike)
                .Include(p => p.Recepti)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<Pregled>> GetTodaysPreglediAsync()
        {
            var today = DateTime.Today;
            return await _dbSet
                .Where(p => p.DatumPregleda == today)
                .Include(p => p.Pacijent)
                .OrderBy(p => p.VrijemePregleda)
                .ToListAsync();
        }

        public async Task<IEnumerable<Pregled>> GetUpcomingPreglediAsync(int days = 7)
        {
            var today = DateTime.Today;
            var endDate = today.AddDays(days);

            return await _dbSet
                .Where(p => p.DatumPregleda >= today && p.DatumPregleda <= endDate)
                .Include(p => p.Pacijent)
                .OrderBy(p => p.DatumPregleda)
                .ThenBy(p => p.VrijemePregleda)
                .ToListAsync();
        }
    }
}
