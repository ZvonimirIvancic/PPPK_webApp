using PrviProjekt.Models;

namespace PrviProjekt.Repositories
{
    public interface IReceptiRepository : IRepository<Recept>
    {
        Task<IEnumerable<Recept>> GetByPacijentIdAsync(int pacijentId);
        Task<IEnumerable<Recept>> GetByPregledIdAsync(int pregledId);
        Task<IEnumerable<Recept>> GetValidReceptiAsync();
        Task<IEnumerable<Recept>> GetExpiringReceptiAsync(int daysAhead = 30);
        Task<IEnumerable<Recept>> GetExpiredReceptiAsync();
        Task<IEnumerable<Recept>> GetByLijekAsync(string nazivLijeka);
        Task<IEnumerable<Recept>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate);
        Task<Dictionary<string, int>> GetLijekStatisticsAsync();
        Task<int> GetActiveReceptiCountAsync();
    }
}
