using DrugiProjekt.Models;

namespace DrugiProjekt.Repositories
{
    public interface ICancerCohortRepository
    {
        Task<CancerCohort?> GetByIdAsync(string id);
        Task<IEnumerable<CancerCohort>> GetAllAsync();
        Task<CancerCohort?> GetByNameAsync(string cohortName);
        Task<IEnumerable<CancerCohort>> GetByCancerTypeAsync(string cancerType);
        Task<IEnumerable<CancerCohort>> GetByStatusAsync(CohortStatus status);
        Task<CancerCohort> InsertAsync(CancerCohort cohort);
        Task<bool> UpdateAsync(string id, CancerCohort cohort);
        Task<bool> DeleteAsync(string id);
        Task<long> CountAsync();
        Task<long> CountByStatusAsync(CohortStatus status);
        Task<IEnumerable<CancerCohort>> GetProcessedCohortsAsync();
        Task<IEnumerable<CancerCohort>> GetFailedCohortsAsync();
        Task<Dictionary<CohortStatus, int>> GetStatusStatisticsAsync();
        Task<Dictionary<string, int>> GetCancerTypeStatisticsAsync();
        Task<IEnumerable<CancerCohort>> GetCohortsWithPatientCountAsync(int minPatients = 1);
        Task<long> GetTotalPatientsAcrossCohortsAsync();
        Task<bool> CohortExistsAsync(string cohortName);
    }
}
