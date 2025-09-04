namespace PrviProjekt.DTOs
{
    public class MedicinskiKartonDto
    {
        public PacijentDto Pacijent { get; set; } = new();
        public List<MedicinskaDokumentacijaDto> MedicinskaDokumentacija { get; set; } = new();
        public List<PregledDto> Pregledi { get; set; } = new();
        public List<ReceptDto> Recepti { get; set; } = new();
    }
}
