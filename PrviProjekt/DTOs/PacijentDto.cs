namespace PrviProjekt.DTOs
{
    public class PacijentDto
    {
        public int Id { get; set; }
        public string OIB { get; set; } = string.Empty;
        public string Ime { get; set; } = string.Empty;
        public string Prezime { get; set; } = string.Empty;
        public string PunoIme { get; set; } = string.Empty;
        public DateTime DatumRodenja { get; set; }
        public string Spol { get; set; } = string.Empty;
        public int Godine { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
