namespace PrviProjekt.DTOs
{
    public class ReceptDto
    {
        public int Id { get; set; }
        public int PacijentId { get; set; }
        public int? PregledId { get; set; }
        public string NazivLijeka { get; set; } = string.Empty;
        public string Doza { get; set; } = string.Empty;
        public string? Upute { get; set; }
        public DateTime DatumIzdavanja { get; set; }
        public DateTime? DatumVazenja { get; set; }
        public bool JeVazeci { get; set; }
        public int DanaDoIsteka { get; set; }
    }
}
