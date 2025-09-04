using DrugiProjekt.Models;

namespace DrugiProjekt.Services
{
    public interface IXenaScraperService
    {
        Task<IEnumerable<CancerCohort>> DiscoverTCGACohortsAsync();
        Task<string> DownloadIlluminaFileAsync(CancerCohort cohort, string downloadPath);
        Task<bool> ExtractGzFileAsync(string gzFilePath, string extractPath);
        Task<IEnumerable<string>> GetAvailableCohortUrlsAsync();
        Task<CancerCohort?> GetCohortDetailsAsync(string cohortUrl);
        Task<bool> ValidateIlluminaFileAsync(string cohortUrl);
    }
}
