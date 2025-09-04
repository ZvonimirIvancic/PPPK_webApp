namespace PrviProjekt.DTOs
{
    public class PregledDto
    {
        public int Id { get; set; }
        public int PacijentId { get; set; }
        public string TipPregleda { get; set; } = string.Empty;
        public string TipPregledaOpis { get; set; } = string.Empty;
        public DateTime DatumPregleda { get; set; }
        public TimeSpan VrijemePregleda { get; set; }
        public DateTime DatumVrijemePregleda { get; set; }
        public string? Opis { get; set; }
        public string? Nalaz { get; set; }
        public int BrojSlika { get; set; }
    }
}
