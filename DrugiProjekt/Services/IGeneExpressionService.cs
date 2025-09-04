using DrugiProjekt.Models;

namespace DrugiProjekt.Services
{
    public interface IGeneExpressionService
    {
        Task<IEnumerable<PatientGeneExpression>> ParseTSVFileAsync(string filePath, string cohortName);
        Task<PatientGeneExpression?> ExtractcGAS_STING_GenesAsync(string patientId, Dictionary<string, double> allGenes);
        Task<bool> ValidateTSVFormatAsync(string filePath);
        Task<Dictionary<string, int>> GetGeneStatisticsAsync(string filePath);
        Task<IEnumerable<string>> GetPatientIdsFromFileAsync(string filePath);
        Task<IEnumerable<string>> GetGeneNamesFromFileAsync(string filePath);
        Task<double> GetGeneExpressionValueAsync(string filePath, string patientId, string geneName);
    }
}
