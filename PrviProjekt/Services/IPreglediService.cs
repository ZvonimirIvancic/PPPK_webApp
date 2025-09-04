using PrviProjekt.DTOs;
using PrviProjekt.Enums;

namespace PrviProjekt.Services
{
    public interface IPreglediService
    {
        Task<IEnumerable<PregledDto>> GetAllPreglediAsync();
        Task<PregledDto?> GetPregledByIdAsync(int id);
        Task<IEnumerable<PregledDto>> GetPreglediByPacijentIdAsync(int pacijentId);
        Task<IEnumerable<PregledDto>> GetPreglediByTipAsync(TipPregleda tipPregleda);
        Task<IEnumerable<PregledDto>> GetPreglediByDateRangeAsync(DateTime fromDate, DateTime toDate);
        Task<IEnumerable<PregledDto>> GetTodaysPreglediAsync();
        Task<IEnumerable<PregledDto>> GetUpcomingPreglediAsync(int days = 7);
        Task<PregledDto> CreatePregledAsync(CreatePregledDto createDto);
        Task<PregledDto> UpdatePregledAsync(int id, UpdatePregledDto updateDto);
        Task DeletePregledAsync(int id);
        Task<PregledDto?> GetPregledWithSlikeAsync(int id);
        Task<Dictionary<TipPregleda, int>> GetPregledStatisticsAsync();
        Task<IEnumerable<PregledDto>> GetRecentPreglediAsync(int count = 10);
        Task<bool> IsPregledTimeAvailableAsync(DateTime datum, TimeSpan vrijeme, int? excludeId = null);
    }
}
