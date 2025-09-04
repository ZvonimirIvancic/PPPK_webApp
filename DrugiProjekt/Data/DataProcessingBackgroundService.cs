using DrugiProjekt.Services;

namespace DrugiProjekt.Data
{
    public class DataProcessingBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DataProcessingBackgroundService> _logger;
        private readonly IConfiguration _configuration;
        private readonly TimeSpan _processingInterval;
        private readonly bool _enableAutoProcessing;

        public DataProcessingBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<DataProcessingBackgroundService> logger,
            IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _configuration = configuration;

            // Default to 6 hours if not configured
            var intervalHours = _configuration.GetValue<int>("TCGA:ProcessingIntervalHours", 6);
            _processingInterval = TimeSpan.FromHours(intervalHours);

            _enableAutoProcessing = _configuration.GetValue<bool>("TCGA:EnableAutoProcessing", false);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("TCGA Data Processing Background Service started");

            if (!_enableAutoProcessing)
            {
                _logger.LogInformation("Auto processing is disabled in configuration");
                return;
            }

            // Wait for initial startup
            await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessDataAsync(stoppingToken);

                    _logger.LogInformation($"Next processing scheduled in {_processingInterval.TotalHours} hours");
                    await Task.Delay(_processingInterval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Data processing cancelled");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error in data processing background service");

                    // Wait shorter interval on error before retrying
                    await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
                }
            }

            _logger.LogInformation("TCGA Data Processing Background Service stopped");
        }

        private async Task ProcessDataAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var services = scope.ServiceProvider;

            try
            {
                _logger.LogInformation("Starting scheduled TCGA data collection and processing...");

                var tcgaDataService = services.GetRequiredService<ITCGADataService>();

                // Step 1: Discover new cohorts
                _logger.LogInformation("Step 1: Discovering new TCGA cohorts...");
                var discoveryResult = await tcgaDataService.StartDataCollectionAsync();

                if (!discoveryResult)
                {
                    _logger.LogWarning("Data collection failed, skipping processing step");
                    return;
                }

                // Step 2: Process unprocessed cohorts
                _logger.LogInformation("Step 2: Processing unprocessed cohorts...");
                var processingResult = await tcgaDataService.ProcessAllCohortsAsync();

                if (processingResult)
                {
                    _logger.LogInformation("All cohorts processed successfully");
                }
                else
                {
                    _logger.LogWarning("Some cohorts failed to process");
                }

                // Step 3: Update statistics and cleanup
                await UpdateStatisticsAsync(services);
                await CleanupOldDataAsync(services);

                _logger.LogInformation("Scheduled data processing completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during scheduled data processing");

                // Send notification about processing failure
                await NotifyProcessingFailureAsync(services, ex);
            }
        }

        private async Task UpdateStatisticsAsync(IServiceProvider services)
        {
            try
            {
                _logger.LogInformation("Updating processing statistics...");

                var tcgaDataService = services.GetRequiredService<ITCGADataService>();
                var statistics = await tcgaDataService.GetProcessingStatisticsAsync();

                _logger.LogInformation($"Statistics updated: {statistics.Count} metrics");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating statistics");
            }
        }

        private async Task CleanupOldDataAsync(IServiceProvider services)
        {
            try
            {
                _logger.LogInformation("Cleaning up old temporary data...");

                var retentionDays = _configuration.GetValue<int>("TCGA:DataRetentionDays", 30);
                var cutoffDate = DateTime.UtcNow.AddDays(-retentionDays);

                // Cleanup temporary files
                var tempDirectory = Path.Combine(Path.GetTempPath(), "TCGA_Processing");
                if (Directory.Exists(tempDirectory))
                {
                    var oldFiles = Directory.GetFiles(tempDirectory, "*", SearchOption.AllDirectories)
                        .Where(f => File.GetCreationTime(f) < cutoffDate);

                    foreach (var file in oldFiles)
                    {
                        try
                        {
                            File.Delete(file);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, $"Could not delete old file: {file}");
                        }
                    }
                }

                _logger.LogInformation("Cleanup completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during cleanup");
            }
        }

        private async Task NotifyProcessingFailureAsync(IServiceProvider services, Exception exception)
        {
            try
            {
                // Here you could implement notification logic:
                // - Send email notifications
                // - Write to monitoring systems
                // - Update health check status
                // - Log to external monitoring services

                _logger.LogCritical(exception, "TCGA data processing failed and requires attention");

                // Example: You could implement INotificationService
                // var notificationService = services.GetService<INotificationService>();
                // if (notificationService != null)
                // {
                //     await notificationService.SendAlertAsync("TCGA Processing Failed", exception.Message);
                // }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending failure notification");
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("TCGA Data Processing Background Service is stopping...");
            await base.StopAsync(cancellationToken);
            _logger.LogInformation("TCGA Data Processing Background Service stopped gracefully");
        }
    }
}
