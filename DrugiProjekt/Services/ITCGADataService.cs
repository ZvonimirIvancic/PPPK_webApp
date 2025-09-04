using DrugiProjekt.Models;

namespace DrugiProjekt.Services
{
    public interface ITCGADataService
    {
        Task<IEnumerable<CancerCohort>> GetAllCohortsAsync();
        Task<CancerCohort?> GetCohortByNameAsync(string cohortName);
        Task<bool> StartDataCollectionAsync();
        Task<bool> ProcessCohortAsync(string cohortName);
        Task<bool> ProcessAllCohortsAsync();
        Task<CohortProcessingResult> ProcessTSVFileAsync(string cohortName, string filePath);
        Task<bool> UpdateCohortStatusAsync(string cohortName, CohortStatus status);
        Task<Dictionary<string, object>> GetProcessingStatisticsAsync();
    }
}
