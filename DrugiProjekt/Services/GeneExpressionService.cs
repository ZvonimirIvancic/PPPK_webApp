using DrugiProjekt.Models;
using System.Globalization;

namespace DrugiProjekt.Services
{
    public class GeneExpressionService : IGeneExpressionService
    {
        private readonly ILogger<GeneExpressionService> _logger;
        private readonly IConfiguration _configuration;
        private readonly string[] _cGAS_STING_Genes;

        public GeneExpressionService(ILogger<GeneExpressionService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            _cGAS_STING_Genes = _configuration.GetSection("TCGA:cGAS_STING_Genes").Get<string[]>() ?? Array.Empty<string>();
        }

        public async Task<IEnumerable<PatientGeneExpression>> ParseTSVFileAsync(string filePath, string cohortName)
        {
            try
            {
                _logger.LogInformation($"Parsing TSV file: {filePath} for cohort: {cohortName}");

                var patientExpressions = new List<PatientGeneExpression>();
                var lines = await File.ReadAllLinesAsync(filePath);

                if (lines.Length < 2)
                {
                    _logger.LogWarning($"TSV file {filePath} has insufficient data");
                    return patientExpressions;
                }

                // First line contains gene names
                var geneNames = lines[0].Split('\t').Skip(1).ToArray(); // Skip first column (patient ID)

                _logger.LogInformation($"Found {geneNames.Length} genes in the file");

                // Process each patient (each line after header)
                var processedCount = 0;
                for (int i = 1; i < lines.Length; i++)
                {
                    try
                    {
                        var values = lines[i].Split('\t');
                        if (values.Length != geneNames.Length + 1) // +1 for patient ID column
                        {
                            _logger.LogWarning($"Line {i} has incorrect number of values. Expected: {geneNames.Length + 1}, Got: {values.Length}");
                            continue;
                        }

                        var patientId = values[0];
                        var geneExpressions = new Dictionary<string, double>();

                        // Parse gene expression values
                        for (int j = 0; j < geneNames.Length; j++)
                        {
                            if (double.TryParse(values[j + 1], NumberStyles.Float, CultureInfo.InvariantCulture, out var expressionValue))
                            {
                                geneExpressions[geneNames[j]] = expressionValue;
                            }
                            else
                            {
                                // Handle missing or invalid values
                                geneExpressions[geneNames[j]] = double.NaN;
                            }
                        }

                        // Create patient gene expression record
                        var patientExpression = new PatientGeneExpression
                        {
                            PatientId = patientId,
                            CancerCohort = cohortName,
                            GeneExpressions = geneExpressions,
                            SourceFile = Path.GetFileName(filePath),
                            ProcessingStatus = ProcessingStatus.Processing
                        };

                        // Extract cGAS-STING pathway genes
                        var cGAS_STING_Expression = await ExtractcGAS_STING_GenesAsync(patientId, geneExpressions);
                        if (cGAS_STING_Expression != null)
                        {
                            patientExpression.cGAS_STING_Genes = cGAS_STING_Expression.cGAS_STING_Genes;
                            patientExpression.ProcessingStatus = ProcessingStatus.Completed;
                        }

                        patientExpressions.Add(patientExpression);
                        processedCount++;

                        if (processedCount % 100 == 0)
                        {
                            _logger.LogInformation($"Processed {processedCount} patients so far...");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error processing line {i} in file {filePath}");
                    }
                }

                _logger.LogInformation($"Successfully parsed {processedCount} patients from {filePath}");
                return patientExpressions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error parsing TSV file: {filePath}");
                throw;
            }
        }

        public async Task<PatientGeneExpression?> ExtractcGAS_STING_GenesAsync(string patientId, Dictionary<string, double> allGenes)
        {
            try
            {
                var cGAS_STING_Expression = new cGAS_STING_GenesExpression();

                // Map gene names to property names (handle aliases)
                var geneMapping = new Dictionary<string, string>
                {
                    ["C6orf150"] = "C6orf150_cGAS",
                    ["cGAS"] = "C6orf150_cGAS",
                    ["CCL5"] = "CCL5",
                    ["CXCL10"] = "CXCL10",
                    ["TMEM173"] = "TMEM173_STING",
                    ["STING"] = "TMEM173_STING",
                    ["CXCL9"] = "CXCL9",
                    ["CXCL11"] = "CXCL11",
                    ["NFKB1"] = "NFKB1",
                    ["IKBKE"] = "IKBKE",
                    ["IRF3"] = "IRF3",
                    ["TREX1"] = "TREX1",
                    ["ATM"] = "ATM",
                    ["IL6"] = "IL6",
                    ["IL8"] = "IL8_CXCL8",
                    ["CXCL8"] = "IL8_CXCL8"
                };

                // Extract values for cGAS-STING pathway genes
                foreach (var kvp in geneMapping)
                {
                    var geneName = kvp.Key;
                    var propertyName = kvp.Value;

                    if (allGenes.TryGetValue(geneName, out var expressionValue))
                    {
                        SetPropertyValue(cGAS_STING_Expression, propertyName, expressionValue);
                    }
                }

                var patientExpression = new PatientGeneExpression
                {
                    PatientId = patientId,
                    cGAS_STING_Genes = cGAS_STING_Expression
                };

                return patientExpression;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error extracting cGAS-STING genes for patient {patientId}");
                return null;
            }
        }

        public async Task<bool> ValidateTSVFormatAsync(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    return false;
                }

                var lines = await File.ReadAllLinesAsync(filePath);
                if (lines.Length < 2)
                {
                    return false;
                }

                // Check header format
                var header = lines[0].Split('\t');
                if (header.Length < 2)
                {
                    return false;
                }

                // Check if first column looks like patient IDs
                var firstDataLine = lines[1].Split('\t');
                return firstDataLine.Length == header.Length &&
                       firstDataLine[0].StartsWith("TCGA-", StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error validating TSV format for file: {filePath}");
                return false;
            }
        }

        public async Task<Dictionary<string, int>> GetGeneStatisticsAsync(string filePath)
        {
            try
            {
                var stats = new Dictionary<string, int>();
                var lines = await File.ReadAllLinesAsync(filePath);

                if (lines.Length < 2)
                {
                    return stats;
                }

                var geneNames = lines[0].Split('\t').Skip(1).ToArray();
                stats["TotalGenes"] = geneNames.Length;
                stats["TotalPatients"] = lines.Length - 1; // Exclude header

                // Count cGAS-STING pathway genes present
                var cGAS_STING_Present = geneNames.Count(gene =>
                    _cGAS_STING_Genes.Any(targetGene =>
                        gene.Equals(targetGene, StringComparison.OrdinalIgnoreCase)));

                stats["cGAS_STING_GenesPresent"] = cGAS_STING_Present;
                stats["cGAS_STING_GenesTotalTargets"] = _cGAS_STING_Genes.Length;

                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting gene statistics for file: {filePath}");
                return new Dictionary<string, int>();
            }
        }

        public async Task<IEnumerable<string>> GetPatientIdsFromFileAsync(string filePath)
        {
            try
            {
                var lines = await File.ReadAllLinesAsync(filePath);
                return lines.Skip(1).Select(line => line.Split('\t')[0]).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting patient IDs from file: {filePath}");
                return Enumerable.Empty<string>();
            }
        }

        public async Task<IEnumerable<string>> GetGeneNamesFromFileAsync(string filePath)
        {
            try
            {
                var firstLine = await File.ReadAllLinesAsync(filePath).ContinueWith(t => t.Result.FirstOrDefault());
                if (firstLine != null)
                {
                    return firstLine.Split('\t').Skip(1).ToArray();
                }
                return Enumerable.Empty<string>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting gene names from file: {filePath}");
                return Enumerable.Empty<string>();
            }
        }

        public async Task<double> GetGeneExpressionValueAsync(string filePath, string patientId, string geneName)
        {
            try
            {
                var lines = await File.ReadAllLinesAsync(filePath);
                if (lines.Length < 2)
                {
                    return double.NaN;
                }

                var geneNames = lines[0].Split('\t').Skip(1).ToArray();
                var geneIndex = Array.IndexOf(geneNames, geneName);
                if (geneIndex == -1)
                {
                    return double.NaN;
                }

                var patientLine = lines.Skip(1).FirstOrDefault(line => line.StartsWith(patientId + "\t"));
                if (patientLine != null)
                {
                    var values = patientLine.Split('\t');
                    if (values.Length > geneIndex + 1 &&
                        double.TryParse(values[geneIndex + 1], NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
                    {
                        return value;
                    }
                }

                return double.NaN;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting gene expression value for patient {patientId}, gene {geneName}");
                return double.NaN;
            }
        }

        private static void SetPropertyValue(cGAS_STING_GenesExpression obj, string propertyName, double value)
        {
            var property = typeof(cGAS_STING_GenesExpression).GetProperty(propertyName);
            property?.SetValue(obj, value);
        }
    }
}
