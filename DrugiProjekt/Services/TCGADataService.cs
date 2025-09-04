using DrugiProjekt.Models;
using DrugiProjekt.Repositories;
using System.Diagnostics;

namespace DrugiProjekt.Services
{
    public class TCGADataService : ITCGADataService
    {
        private readonly IXenaScraperService _scraperService;
        private readonly IMinIOStorageService _storageService;
        private readonly IGeneExpressionService _geneExpressionService;
        private readonly ICancerCohortRepository _cohortRepository;
        private readonly IPatientGeneExpressionRepository _patientRepository;
        private readonly ILogger<TCGADataService> _logger;
        private readonly string _tempDirectory;

        public TCGADataService(
            IXenaScraperService scraperService,
            IMinIOStorageService storageService,
            IGeneExpressionService geneExpressionService,
            ICancerCohortRepository cohortRepository,
            IPatientGeneExpressionRepository patientRepository,
            ILogger<TCGADataService> logger)
        {
            _scraperService = scraperService;
            _storageService = storageService;
            _geneExpressionService = geneExpressionService;
            _cohortRepository = cohortRepository;
            _patientRepository = patientRepository;
            _logger = logger;
            _tempDirectory = Path.Combine(Path.GetTempPath(), "TCGA_Processing");

            Directory.CreateDirectory(_tempDirectory);
        }

        public async Task<IEnumerable<CancerCohort>> GetAllCohortsAsync()
        {
            return await _cohortRepository.GetAllAsync();
        }

        public async Task<CancerCohort?> GetCohortByNameAsync(string cohortName)
        {
            return await _cohortRepository.GetByNameAsync(cohortName);
        }

        public async Task<bool> StartDataCollectionAsync()
        {
            try
            {
                _logger.LogInformation("Starting TCGA data collection process...");

                // Discover cohorts from Xena browser
                var discoveredCohorts = await _scraperService.DiscoverTCGACohortsAsync();

                foreach (var cohort in discoveredCohorts)
                {
                    // Check if cohort already exists
                    var existingCohort = await _cohortRepository.GetByNameAsync(cohort.CohortName);
                    if (existingCohort == null)
                    {
                        await _cohortRepository.InsertAsync(cohort);
                        _logger.LogInformation($"Added new cohort: {cohort.CohortName}");
                    }
                    else
                    {
                        // Update existing cohort information
                        existingCohort.IlluminaFileUrl = cohort.IlluminaFileUrl;
                        existingCohort.UpdatedAt = DateTime.UtcNow;
                        await _cohortRepository.UpdateAsync(existingCohort.Id, existingCohort);
                    }
                }

                _logger.LogInformation($"Data collection completed. Found {discoveredCohorts.Count()} cohorts");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during data collection");
                return false;
            }
        }

        public async Task<bool> ProcessCohortAsync(string cohortName)
        {
            try
            {
                _logger.LogInformation($"Starting processing for cohort: {cohortName}");

                var cohort = await _cohortRepository.GetByNameAsync(cohortName);
                if (cohort == null)
                {
                    _logger.LogWarning($"Cohort not found: {cohortName}");
                    return false;
                }

                // Update status to downloading
                await UpdateCohortStatusAsync(cohortName, CohortStatus.Downloading);

                // Download the TSV file
                var downloadPath = Path.Combine(_tempDirectory, cohortName);
                Directory.CreateDirectory(downloadPath);

                var filePath = await _scraperService.DownloadIlluminaFileAsync(cohort, downloadPath);
                if (string.IsNullOrEmpty(filePath))
                {
                    await UpdateCohortStatusAsync(cohortName, CohortStatus.Failed);
                    return false;
                }

                // Extract if it's a gzipped file
                if (filePath.EndsWith(".gz"))
                {
                    var extractSuccess = await _scraperService.ExtractGzFileAsync(filePath, downloadPath);
                    if (!extractSuccess)
                    {
                        await UpdateCohortStatusAsync(cohortName, CohortStatus.Failed);
                        return false;
                    }
                    filePath = filePath.Replace(".gz", "");
                }

                // Update status to downloaded
                await UpdateCohortStatusAsync(cohortName, CohortStatus.Downloaded);

                // Upload to MinIO storage
                var fileName = Path.GetFileName(filePath);
                var bucketName = "tcga-tsv-files";
                var uploadSuccess = await _storageService.UploadTSVFileAsync(bucketName, fileName, filePath);

                if (!uploadSuccess)
                {
                    _logger.LogError($"Failed to upload file to MinIO for cohort: {cohortName}");
                }

                // Process the TSV file
                var result = await ProcessTSVFileAsync(cohortName, filePath);

                // Update cohort with processing results
                cohort.ProcessedPatients = result.ProcessedPatients;
                cohort.TotalPatients = result.TotalPatients;
                cohort.FileSizeBytes = result.FileSizeBytes;
                cohort.ProcessingDate = DateTime.UtcNow;
                cohort.Status = result.Success ? CohortStatus.Completed : CohortStatus.Failed;

                await _cohortRepository.UpdateAsync(cohort.Id, cohort);

                // Cleanup temporary files
                try
                {
                    Directory.Delete(downloadPath, true);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, $"Failed to cleanup temporary directory: {downloadPath}");
                }

                _logger.LogInformation($"Processing completed for cohort: {cohortName}. Success: {result.Success}");
                return result.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing cohort: {cohortName}");
                await UpdateCohortStatusAsync(cohortName, CohortStatus.Failed);
                return false;
            }
        }

        public async Task<bool> ProcessAllCohortsAsync()
        {
            var cohorts = await _cohortRepository.GetAllAsync();
            var unprocessedCohorts = cohorts.Where(c => c.Status != CohortStatus.Completed);

            var results = new List<bool>();
            foreach (var cohort in unprocessedCohorts)
            {
                var result = await ProcessCohortAsync(cohort.CohortName);
                results.Add(result);

                // Add delay between processing to avoid overwhelming the system
                await Task.Delay(TimeSpan.FromSeconds(30));
            }

            return results.All(r => r);
        }

        public async Task<CohortProcessingResult> ProcessTSVFileAsync(string cohortName, string filePath)
        {
            var stopwatch = Stopwatch.StartNew();
            var result = new CohortProcessingResult();

            try
            {
                _logger.LogInformation($"Processing TSV file for cohort: {cohortName}");

                // Update status to processing
                await UpdateCohortStatusAsync(cohortName, CohortStatus.Processing);

                // Validate TSV format
                var isValid = await _geneExpressionService.ValidateTSVFormatAsync(filePath);
                if (!isValid)
                {
                    result.ErrorMessage = "Invalid TSV format";
                    return result;
                }

                // Get file size
                var fileInfo = new FileInfo(filePath);
                result.FileSizeBytes = fileInfo.Length;

                // Parse the TSV file
                var patientExpressions = await _geneExpressionService.ParseTSVFileAsync(filePath, cohortName);
                var patientsList = patientExpressions.ToList();

                result.TotalPatients = patientsList.Count;

                if (patientsList.Any())
                {
                    // Insert patients in batches to avoid memory issues
                    const int batchSize = 100;
                    var processedCount = 0;

                    for (int i = 0; i < patientsList.Count; i += batchSize)
                    {
                        var batch = patientsList.Skip(i).Take(batchSize);
                        await _patientRepository.InsertManyAsync(batch);

                        processedCount += batch.Count();
                        _logger.LogInformation($"Processed batch {i / batchSize + 1} for cohort {cohortName}: {processedCount}/{patientsList.Count} patients");
                    }

                    result.ProcessedPatients = processedCount;
                    result.Success = true;
                }

                stopwatch.Stop();
                result.ProcessingTime = stopwatch.Elapsed;

                _logger.LogInformation($"Successfully processed {result.ProcessedPatients} patients from cohort {cohortName} in {result.ProcessingTime.TotalMinutes:F2} minutes");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing TSV file for cohort: {cohortName}");
                result.ErrorMessage = ex.Message;
                result.Success = false;
            }

            return result;
        }

        public async Task<bool> UpdateCohortStatusAsync(string cohortName, CohortStatus status)
        {
            try
            {
                var cohort = await _cohortRepository.GetByNameAsync(cohortName);
                if (cohort != null)
                {
                    cohort.Status = status;
                    cohort.UpdatedAt = DateTime.UtcNow;
                    return await _cohortRepository.UpdateAsync(cohort.Id, cohort);
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating cohort status for: {cohortName}");
                return false;
            }
        }

        public async Task<Dictionary<string, object>> GetProcessingStatisticsAsync()
        {
            try
            {
                var cohorts = await _cohortRepository.GetAllAsync();
                var totalPatients = await _patientRepository.CountAsync();
                var cGAS_STING_Records = (await _patientRepository.GetcGAS_STING_DataAsync()).Count();

                return new Dictionary<string, object>
                {
                    ["TotalCohorts"] = cohorts.Count(),
                    ["CompletedCohorts"] = cohorts.Count(c => c.Status == CohortStatus.Completed),
                    ["ProcessingCohorts"] = cohorts.Count(c => c.Status == CohortStatus.Processing),
                    ["FailedCohorts"] = cohorts.Count(c => c.Status == CohortStatus.Failed),
                    ["TotalPatients"] = totalPatients,
                    ["cGAS_STING_Records"] = cGAS_STING_Records,
                    ["LastUpdate"] = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting processing statistics");
                return new Dictionary<string, object>();
            }
        }
    }
}
