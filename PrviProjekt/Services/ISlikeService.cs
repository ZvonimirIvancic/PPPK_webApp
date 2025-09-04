using Microsoft.AspNetCore.Components.Forms;
using PrviProjekt.DTOs;

namespace PrviProjekt.Services
{
    public interface ISlikeService
    {
        Task<IEnumerable<SlikaDto>> GetByPregledIdAsync(int pregledId);
        Task<SlikaDto?> GetByIdAsync(int id);
        Task<SlikaDto> UploadSlikaAsync(int pregledId, IBrowserFile file, string opis = "");
        Task<bool> DeleteSlikaAsync(int id);
        Task<byte[]> GetSlikaContentAsync(int id);
        Task<IEnumerable<SlikaDto>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate);
        Task<Dictionary<string, int>> GetFileTypeStatisticsAsync();
        Task<long> GetTotalStorageUsedAsync();
    }
}
