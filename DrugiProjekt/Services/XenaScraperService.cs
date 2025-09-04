using DrugiProjekt.Models;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System.IO.Compression;

namespace DrugiProjekt.Services
{
    public class XenaScraperService : IXenaScraperService, IDisposable
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<XenaScraperService> _logger;
        private readonly IWebDriver _driver;
        private readonly HttpClient _httpClient;

        public XenaScraperService(IConfiguration configuration, ILogger<XenaScraperService> logger)
        {
            _configuration = configuration;
            _logger = logger;

            // Initialize Chrome driver with headless options
            var chromeOptions = new ChromeOptions();
            chromeOptions.AddArguments("--headless", "--no-sandbox", "--disable-dev-shm-usage");
            _driver = new ChromeDriver(chromeOptions);

            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(_configuration.GetValue<int>("TCGA:DownloadTimeout", 300))
            };
        }

        public async Task<IEnumerable<CancerCohort>> DiscoverTCGACohortsAsync()
        {
            var cohorts = new List<CancerCohort>();
            var baseUrl = _configuration["TCGA:XenaBaseUrl"] + "?host=https%3A%2F%2Ftcga.xenahubs.net&removeHub=https%3A%2F%2Fxena.treehouse.gi.ucsc.edu%3A443";

            try
            {
                _logger.LogInformation("Navigating to TCGA Xena browser page...");
                _driver.Navigate().GoToUrl(baseUrl);

                // Wait for page to load
                var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(30));
                wait.Until(drv => drv.FindElement(By.TagName("ul")));

                // Find all TCGA cohort links
                var cohortLinks = _driver.FindElements(By.XPath("//ul/li/a[@href]"))
                    .Where(link => link.GetAttribute("href").Contains("TCGA"))
                    .Select(link => link.GetAttribute("href"))
                    .ToList();

                _logger.LogInformation($"Found {cohortLinks.Count} TCGA cohort links");

                foreach (var cohortUrl in cohortLinks)
                {
                    try
                    {
                        var cohort = await ProcessCohortPageAsync(cohortUrl);
                        if (cohort != null)
                        {
                            cohorts.Add(cohort);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error processing cohort URL: {cohortUrl}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error discovering TCGA cohorts");
                throw;
            }

            return cohorts;
        }

        private async Task<CancerCohort?> ProcessCohortPageAsync(string cohortUrl)
        {
            _logger.LogInformation($"Processing cohort: {cohortUrl}");
            _driver.Navigate().GoToUrl(cohortUrl);

            await Task.Delay(1000); // Wait for page load

            try
            {
                // Look for gene expression RNAseq section
                var geneExpressionSection = _driver.FindElement(
                    By.XPath("//div[h3[contains(text(), 'gene expression RNAseq')]]"));

                // Look for IlluminaHiSeq pancan normalized link
                var illuminaLink = FindIlluminaLink(geneExpressionSection);
                if (string.IsNullOrEmpty(illuminaLink))
                {
                    _logger.LogWarning($"No IlluminaHiSeq pancan normalized link found for {cohortUrl}");
                    return null;
                }

                // Extract cohort name from URL
                var cohortName = ExtractCohortNameFromUrl(cohortUrl);
                var cancerType = ExtractCancerTypeFromName(cohortName);

                var cohort = new CancerCohort
                {
                    CohortName = cohortName,
                    CancerType = cancerType,
                    XenaUrl = cohortUrl,
                    IlluminaFileUrl = illuminaLink,
                    Status = CohortStatus.Discovered,
                    Description = $"TCGA {cancerType} cohort data"
                };

                _logger.LogInformation($"Successfully processed cohort: {cohortName}");
                return cohort;
            }
            catch (NoSuchElementException)
            {
                _logger.LogWarning($"No gene expression RNAseq section found for {cohortUrl}");
                return null;
            }
        }

        private string? FindIlluminaLink(IWebElement geneExpressionSection)
        {
            try
            {
                var ulElement = geneExpressionSection.FindElement(By.XPath(".//ul"));
                var illuminaElement = ulElement.FindElement(
                    By.XPath(".//li/a[contains(text(), 'IlluminaHiSeq') and contains(text(), 'pancan')]"));

                return illuminaElement?.GetAttribute("href");
            }
            catch (NoSuchElementException)
            {
                return null;
            }
        }

        public async Task<string> DownloadIlluminaFileAsync(CancerCohort cohort, string downloadPath)
        {
            try
            {
                _logger.LogInformation($"Downloading file for cohort {cohort.CohortName}...");

                // Navigate to the Illumina link page to get the actual download URL
                _driver.Navigate().GoToUrl(cohort.IlluminaFileUrl);
                await Task.Delay(2000);

                var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(30));
                wait.Until(drv => drv.FindElement(By.TagName("a")));

                // Find the download link
                var downloadLink = _driver.FindElement(By.XPath("//span/a[contains(text(), 'download')]"));
                var fileUrl = downloadLink.GetAttribute("href");
                var fileName = Path.GetFileName(new Uri(fileUrl).AbsolutePath);
                var filePath = Path.Combine(downloadPath, fileName);

                // Download the file
                using var response = await _httpClient.GetAsync(fileUrl);
                response.EnsureSuccessStatusCode();

                await using var fileStream = File.Create(filePath);
                await response.Content.CopyToAsync(fileStream);

                _logger.LogInformation($"File downloaded successfully: {filePath}");
                return filePath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error downloading file for cohort {cohort.CohortName}");
                throw;
            }
        }

        public async Task<bool> ExtractGzFileAsync(string gzFilePath, string extractPath)
        {
            try
            {
                _logger.LogInformation($"Extracting file: {gzFilePath}");

                await using var fileStream = File.OpenRead(gzFilePath);
                await using var gzipStream = new GZipStream(fileStream, CompressionMode.Decompress);

                var extractedFileName = Path.GetFileNameWithoutExtension(gzFilePath);
                var extractedFilePath = Path.Combine(extractPath, extractedFileName);

                await using var outputStream = File.Create(extractedFilePath);
                await gzipStream.CopyToAsync(outputStream);

                _logger.LogInformation($"File extracted successfully: {extractedFilePath}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error extracting file: {gzFilePath}");
                return false;
            }
        }

        public async Task<IEnumerable<string>> GetAvailableCohortUrlsAsync()
        {
            var cohorts = await DiscoverTCGACohortsAsync();
            return cohorts.Select(c => c.XenaUrl);
        }

        public async Task<CancerCohort?> GetCohortDetailsAsync(string cohortUrl)
        {
            return await ProcessCohortPageAsync(cohortUrl);
        }

        public async Task<bool> ValidateIlluminaFileAsync(string cohortUrl)
        {
            try
            {
                _driver.Navigate().GoToUrl(cohortUrl);
                await Task.Delay(1000);

                var geneExpressionSection = _driver.FindElement(
                    By.XPath("//div[h3[contains(text(), 'gene expression RNAseq')]]"));

                var illuminaLink = FindIlluminaLink(geneExpressionSection);
                return !string.IsNullOrEmpty(illuminaLink);
            }
            catch
            {
                return false;
            }
        }

        private static string ExtractCohortNameFromUrl(string url)
        {
            try
            {
                var uri = new Uri(url);
                var queryParams = System.Web.HttpUtility.ParseQueryString(uri.Query);
                var dataset = queryParams["dataset"];

                if (!string.IsNullOrEmpty(dataset))
                {
                    return dataset.Split('.')[0]; // Extract TCGA cohort code
                }

                return "Unknown";
            }
            catch
            {
                return "Unknown";
            }
        }

        private static string ExtractCancerTypeFromName(string cohortName)
        {
            // Map TCGA cohort codes to cancer types
            var cancerTypes = new Dictionary<string, string>
            {
                ["BRCA"] = "Breast invasive carcinoma",
                ["LUAD"] = "Lung adenocarcinoma",
                ["LUSC"] = "Lung squamous cell carcinoma",
                ["PRAD"] = "Prostate adenocarcinoma",
                ["THCA"] = "Thyroid carcinoma",
                ["COAD"] = "Colon adenocarcinoma",
                ["STAD"] = "Stomach adenocarcinoma",
                ["BLCA"] = "Bladder urothelial carcinoma",
                ["LIHC"] = "Liver hepatocellular carcinoma",
                ["CESC"] = "Cervical squamous cell carcinoma and endocervical adenocarcinoma",
                ["KIRP"] = "Kidney renal papillary cell carcinoma",
                ["SARC"] = "Sarcoma",
                ["LAML"] = "Acute myeloid leukemia",
                ["PAAD"] = "Pancreatic adenocarcinoma",
                ["PCPG"] = "Pheochromocytoma and paraganglioma",
                ["READ"] = "Rectum adenocarcinoma",
                ["TGCT"] = "Testicular germ cell tumors",
                ["THYM"] = "Thymoma",
                ["KICH"] = "Kidney chromophobe",
                ["KIRC"] = "Kidney renal clear cell carcinoma",
                ["ACC"] = "Adrenocortical carcinoma",
                ["MESO"] = "Mesothelioma",
                ["UVM"] = "Uveal melanoma"
            };

            return cancerTypes.TryGetValue(cohortName, out var cancerType) ? cancerType : cohortName;
        }

        public void Dispose()
        {
            _driver?.Quit();
            _driver?.Dispose();
            _httpClient?.Dispose();
        }
    }
}
