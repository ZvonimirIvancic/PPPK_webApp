using Microsoft.EntityFrameworkCore;
using PrviProjekt.Data;
using PrviProjekt.Models;

namespace PrviProjekt.Repositories
{
    public class PacijentRepository : Repository<Pacijent>, IPacijentRepository
    {
        public PacijentRepository(MedicinskiDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Pacijent>> SearchByPrezimeOrOibAsync(string searchTerm)
        {
            return await _dbSet
                .Where(p => p.Prezime.ToLower().Contains(searchTerm.ToLower()) ||
                           p.OIB == searchTerm)
                .OrderBy(p => p.Prezime)
                .ThenBy(p => p.Ime)
                .ToListAsync();
        }

        public async Task<Pacijent?> GetByOibAsync(string oib)
        {
            return await _dbSet.FirstOrDefaultAsync(p => p.OIB == oib);
        }

        public async Task<Pacijent?> GetWithMedicinskaDokumentacijaAsync(int id)
        {
            return await _dbSet
                .Include(p => p.MedicinskaDokumentacija.OrderByDescending(md => md.DatumPocetka))
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Pacijent?> GetWithPreglediAsync(int id)
        {
            return await _dbSet
                .Include(p => p.Pregledi.OrderByDescending(pr => pr.DatumPregleda))
                .ThenInclude(pr => pr.Slike)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Pacijent?> GetWithReceptiAsync(int id)
        {
            return await _dbSet
                .Include(p => p.Recepti.OrderByDescending(r => r.DatumIzdavanja))
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Pacijent?> GetCompletePatientDataAsync(int id)
        {
            return await _dbSet
                .Include(p => p.MedicinskaDokumentacija.OrderByDescending(md => md.DatumPocetka))
                .Include(p => p.Pregledi.OrderByDescending(pr => pr.DatumPregleda))
                    .ThenInclude(pr => pr.Slike)
                .Include(p => p.Pregledi)
                    .ThenInclude(pr => pr.Recepti)
                .Include(p => p.Recepti.OrderByDescending(r => r.DatumIzdavanja))
                    .ThenInclude(r => r.Pregled)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<bool> OibExistsAsync(string oib, int? excludeId = null)
        {
            var query = _dbSet.Where(p => p.OIB == oib);
            if (excludeId.HasValue)
            {
                query = query.Where(p => p.Id != excludeId.Value);
            }
            return await query.AnyAsync();
        }
    }
}
