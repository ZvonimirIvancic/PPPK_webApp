using DrugiProjekt.Models;

namespace DrugiProjekt.Repositories
{
    public interface IPatientGeneExpressionRepository
    {
        Task<PatientGeneExpression?> GetByIdAsync(string id);
        Task<IEnumerable<PatientGeneExpression>> GetAllAsync();
        Task<IEnumerable<PatientGeneExpression>> GetByCohortAsync(string cohort);
        Task<IEnumerable<PatientGeneExpression>> GetByPatientIdAsync(string patientId);
        Task<PatientGeneExpression> InsertAsync(PatientGeneExpression patient);
        Task<IEnumerable<PatientGeneExpression>> InsertManyAsync(IEnumerable<PatientGeneExpression> patients);
        Task<bool> UpdateAsync(string id, PatientGeneExpression patient);
        Task<bool> DeleteAsync(string id);
        Task<long> CountAsync();
        Task<long> CountByCohortAsync(string cohort);
        Task<IEnumerable<PatientGeneExpression>> GetByGeneExpressionRangeAsync(string geneName, double minValue, double maxValue);
        Task<IEnumerable<PatientGeneExpression>> GetcGAS_STING_DataAsync(string? cohort = null);
        Task<Dictionary<string, double>> GetGeneExpressionStatisticsAsync(string geneName, string? cohort = null);
        Task<IEnumerable<string>> GetAvailableCohortsAsync();
        Task<IEnumerable<string>> GetPatientIdsByCohortAsync(string cohort);
    }
}
