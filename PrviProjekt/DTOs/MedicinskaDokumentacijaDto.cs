namespace PrviProjekt.DTOs
{
    public class MedicinskaDokumentacijaDto
    {
        public int Id { get; set; }
        public int PacijentId { get; set; }
        public string NazivBolesti { get; set; } = string.Empty;
        public DateTime DatumPocetka { get; set; }
        public DateTime? DatumZavrsetka { get; set; }
        public string? Opis { get; set; }
        public bool JeAktivna { get; set; }
        public int TrajanjeUDanima { get; set; }
    }
}
