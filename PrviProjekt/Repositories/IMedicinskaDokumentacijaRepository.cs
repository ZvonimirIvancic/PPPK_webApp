using PrviProjekt.Models;

namespace PrviProjekt.Repositories
{
    public interface IMedicinskaDokumentacijaRepository : IRepository<MedicinskaDokumentacija>
    {
        Task<IEnumerable<MedicinskaDokumentacija>> GetByPacijentIdAsync(int pacijentId);
        Task<IEnumerable<MedicinskaDokumentacija>> GetActiveBoljestiAsync(int pacijentId);
        Task<IEnumerable<MedicinskaDokumentacija>> GetByBolestAsync(string nazivBolesti);
        Task<IEnumerable<MedicinskaDokumentacija>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate);
        Task<IEnumerable<MedicinskaDokumentacija>> GetLongestActiveBoljestiAsync(int count = 10);
        Task<Dictionary<string, int>> GetBolestStatisticsAsync();
        Task<Dictionary<string, double>> GetAverageTrajanjeByBolestAsync();
        Task<IEnumerable<MedicinskaDokumentacija>> GetRecentlyCompletedAsync(int days = 30);
    }
}
