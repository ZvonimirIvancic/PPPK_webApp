namespace DrugiProjekt.Services
{
    public class CohortProcessingResult
    {
        public bool Success { get; set; }
        public int ProcessedPatients { get; set; }
        public int TotalPatients { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public TimeSpan ProcessingTime { get; set; }
        public long FileSizeBytes { get; set; }
    }
}
