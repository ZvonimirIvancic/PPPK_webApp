namespace DrugiProjekt.Models
{
    public class VisualizationResult
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Title { get; set; } = string.Empty;
        public string ChartType { get; set; } = string.Empty;
        public object Data { get; set; } = new();
        public Dictionary<string, object> Config { get; set; } = new();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string[] Labels { get; set; } = Array.Empty<string>();
        public Dictionary<string, object> Metadata { get; set; } = new();
    }
}
