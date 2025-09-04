using DrugiProjekt.Models;

namespace DrugiProjekt.Services
{
    public interface IVisualizationService
    {
        Task<VisualizationResult> CreateGeneExpressionHistogramAsync(string geneName, string? cohort = null);
        Task<VisualizationResult> CreatecGAS_STING_HeatmapAsync(string cohort);
        Task<VisualizationResult> CreateCohortComparisonChartAsync(string[] cohorts, string geneName);
        Task<VisualizationResult> CreateGeneCorrelationPlotAsync(string[] geneNames, string? cohort = null);
        Task<VisualizationResult> CreatePatientExpressionProfileAsync(string patientId);
        Task<VisualizationResult> CreateBoxPlotAsync(string geneName, string[] cohorts);
        Task<VisualizationResult> CreateScatterPlotAsync(string geneX, string geneY, string? cohort = null);
        Task<VisualizationResult> CreateTopGenesBarChartAsync(string cohort, int topCount = 20);
        Task<VisualizationResult> CreatePCAAnalysisAsync(string cohort, string[] geneNames);
        Task<VisualizationResult> CreateSurvivalAnalysisAsync(string cohort, string geneName, double threshold);
        Task<ChartDataResult> GetChartDataForGeneAsync(string geneName, string? cohort = null);
        Task<ChartDataResult> GetcGAS_STING_ChartDataAsync(string cohort);
        Task<Dictionary<string, object>> GetVisualizationStatisticsAsync(string cohort);
    }
}
