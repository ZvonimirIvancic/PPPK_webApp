using PrviProjekt.DTOs;

namespace PrviProjekt.Services
{
    public interface IReceptiService
    {
        Task<IEnumerable<ReceptDto>> GetAllReceptiAsync();
        Task<ReceptDto?> GetReceptByIdAsync(int id);
        Task<IEnumerable<ReceptDto>> GetReceptiByPacijentIdAsync(int pacijentId);
        Task<IEnumerable<ReceptDto>> GetValidReceptiAsync();
        Task<IEnumerable<ReceptDto>> GetExpiringReceptiAsync(int daysAhead = 30);
        Task<IEnumerable<ReceptDto>> SearchReceptiByLijekAsync(string nazivLijeka);
        Task<ReceptDto> CreateReceptAsync(CreateReceptDto createDto);
        Task<ReceptDto> UpdateReceptAsync(int id, UpdateReceptDto updateDto);
        Task DeleteReceptAsync(int id);
        Task<Dictionary<string, int>> GetLijekStatisticsAsync();
        Task<int> GetActiveReceptiCountAsync();
    }
}
