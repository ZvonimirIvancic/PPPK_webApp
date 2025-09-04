namespace DrugiProjekt.Models
{
    public class Dataset
    {
        public string Label { get; set; } = string.Empty;
        public double[] Data { get; set; } = Array.Empty<double>();
        public string BackgroundColor { get; set; } = string.Empty;
        public string BorderColor { get; set; } = string.Empty;
        public int BorderWidth { get; set; } = 1;
    }
}
