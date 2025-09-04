using PrviProjekt.Models;

namespace PrviProjekt.Repositories
{
    public interface ISlikeRepository : IRepository<Slika>
    {
        Task<IEnumerable<Slika>> GetByPregledIdAsync(int pregledId);
        Task<Slika?> GetByFileNameAsync(string fileName);
        Task<IEnumerable<Slika>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate);
        Task<long> GetTotalFileSizeAsync();
        Task<IEnumerable<Slika>> GetByFileSizeRangeAsync(long minSize, long maxSize);
        Task<IEnumerable<Slika>> GetByFileTypeAsync(string fileType);
        Task<bool> FileNameExistsAsync(string fileName, int? excludeId = null);
        Task<Dictionary<string, int>> GetFileTypeStatisticsAsync();
    }
}
