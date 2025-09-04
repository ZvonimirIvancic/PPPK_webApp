namespace PrviProjekt.DTOs
{
    public class UpdatePacijentDto
    {
        public string OIB { get; set; } = string.Empty;
        public string Ime { get; set; } = string.Empty;
        public string Prezime { get; set; } = string.Empty;
        public DateTime DatumRodenja { get; set; }
        public string Spol { get; set; } = string.Empty;
    }
}
