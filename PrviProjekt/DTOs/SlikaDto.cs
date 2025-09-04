namespace PrviProjekt.DTOs
{
    public class SlikaDto
    {
        public int Id { get; set; }
        public int PregledId { get; set; }
        public string NazivDatoteke { get; set; } = string.Empty;
        public string Putanja { get; set; } = string.Empty;
        public string? TipDatoteke { get; set; }
        public long? VelicinaDatoteke { get; set; }
        public string VelicinaFormatirana { get; set; } = string.Empty;
        public DateTime DatumUpload { get; set; }
        public string? Opis { get; set; }
        public string PacijentPunoIme { get; set; } = string.Empty;
    }
}
