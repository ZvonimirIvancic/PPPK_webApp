using PrviProjekt.Enums;
using PrviProjekt.Models;

namespace PrviProjekt.Repositories
{
    public interface IPreglediRepository : IRepository<Pregled>
    {
        Task<IEnumerable<Pregled>> GetByPacijentIdAsync(int pacijentId);
        Task<IEnumerable<Pregled>> GetByTipPregledaAsync(TipPregleda tipPregleda);
        Task<IEnumerable<Pregled>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate);
        Task<Pregled?> GetWithSlikeAsync(int id);
        Task<IEnumerable<Pregled>> GetTodaysPreglediAsync();
        Task<IEnumerable<Pregled>> GetUpcomingPreglediAsync(int days = 7);
    }
}
