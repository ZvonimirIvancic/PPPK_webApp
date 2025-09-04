using System.Data;


namespace DrugiProjekt.Models
{
    public class ChartDataResult
    {
        public string[] Labels { get; set; } = Array.Empty<string>();
        public Dataset[] Datasets { get; set; } = Array.Empty<Dataset>();
        public Dictionary<string, object> Options { get; set; } = new();
    }
}
