using Microsoft.EntityFrameworkCore;
using PrviProjekt.Data;
using PrviProjekt.Models;

namespace PrviProjekt.Repositories
{
    public class SlikeRepository : Repository<Slika>, ISlikeRepository
    {
        public SlikeRepository(MedicinskiDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Slika>> GetByPregledIdAsync(int pregledId)
        {
            return await _dbSet
                .Where(s => s.PregledId == pregledId)
                .Include(s => s.Pregled)
                .ThenInclude(p => p.Pacijent)
                .OrderByDescending(s => s.DatumUpload)
                .ToListAsync();
        }

        public async Task<Slika?> GetByFileNameAsync(string fileName)
        {
            return await _dbSet
                .Include(s => s.Pregled)
                .ThenInclude(p => p.Pacijent)
                .FirstOrDefaultAsync(s => s.NazivDatoteke == fileName);
        }

        public async Task<IEnumerable<Slika>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate)
        {
            return await _dbSet
                .Where(s => s.DatumUpload >= fromDate && s.DatumUpload <= toDate)
                .Include(s => s.Pregled)
                .ThenInclude(p => p.Pacijent)
                .OrderByDescending(s => s.DatumUpload)
                .ToListAsync();
        }

        public async Task<long> GetTotalFileSizeAsync()
        {
            return await _dbSet
                .Where(s => s.VelicinaDatoteke.HasValue)
                .SumAsync(s => s.VelicinaDatoteke.Value);
        }

        public async Task<IEnumerable<Slika>> GetByFileSizeRangeAsync(long minSize, long maxSize)
        {
            return await _dbSet
                .Where(s => s.VelicinaDatoteke.HasValue &&
                           s.VelicinaDatoteke >= minSize &&
                           s.VelicinaDatoteke <= maxSize)
                .Include(s => s.Pregled)
                .ThenInclude(p => p.Pacijent)
                .OrderByDescending(s => s.VelicinaDatoteke)
                .ToListAsync();
        }

        public async Task<IEnumerable<Slika>> GetByFileTypeAsync(string fileType)
        {
            return await _dbSet
                .Where(s => s.TipDatoteke == fileType)
                .Include(s => s.Pregled)
                .ThenInclude(p => p.Pacijent)
                .OrderByDescending(s => s.DatumUpload)
                .ToListAsync();
        }

        public async Task<bool> FileNameExistsAsync(string fileName, int? excludeId = null)
        {
            var query = _dbSet.Where(s => s.NazivDatoteke == fileName);
            if (excludeId.HasValue)
            {
                query = query.Where(s => s.Id != excludeId.Value);
            }
            return await query.AnyAsync();
        }

        public async Task<Dictionary<string, int>> GetFileTypeStatisticsAsync()
        {
            return await _dbSet
                .Where(s => !string.IsNullOrEmpty(s.TipDatoteke))
                .GroupBy(s => s.TipDatoteke)
                .ToDictionaryAsync(g => g.Key, g => g.Count());
        }

        public override async Task<Slika?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(s => s.Pregled)
                .ThenInclude(p => p.Pacijent)
                .FirstOrDefaultAsync(s => s.Id == id);
        }
    }
}
