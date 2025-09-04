using PrviProjekt.DTOs;

namespace PrviProjekt.Services
{
    public interface IPacijentService
    {
        Task<IEnumerable<PacijentDto>> GetAllPacijentiAsync();
        Task<PacijentDto?> GetPacijentByIdAsync(int id);
        Task<PacijentDto?> GetPacijentByOibAsync(string oib);
        Task<IEnumerable<PacijentDto>> SearchPacijentiAsync(string searchTerm);
        Task<PacijentDto> CreatePacijentAsync(CreatePacijentDto createDto);
        Task<PacijentDto> UpdatePacijentAsync(int id, UpdatePacijentDto updateDto);
        Task DeletePacijentAsync(int id);
        Task<MedicinskiKartonDto?> GetMedicinskiKartonAsync(int pacijentId);
        Task<bool> OibExistsAsync(string oib, int? excludeId = null);
    }
}
