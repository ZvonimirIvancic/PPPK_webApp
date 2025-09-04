using System.ComponentModel.DataAnnotations;

namespace PrviProjekt.Models
{
    public class MedicinskaDokumentacija : BaseEntity
    {
        [Required(ErrorMessage = "Pacijent ID je obavezan")]
        public int PacijentId { get; set; }

        [Required(ErrorMessage = "Naziv bolesti je obavezan")]
        [StringLength(200, ErrorMessage = "Naziv bolesti može imati maksimalno 200 znakova")]
        public string NazivBolesti { get; set; } = string.Empty;

        [Required(ErrorMessage = "Datum početka je obavezan")]
        [DataType(DataType.Date)]
        public DateTime DatumPocetka { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DatumZavrsetka { get; set; }

        [StringLength(1000, ErrorMessage = "Opis može imati maksimalno 1000 znakova")]
        public string? Opis { get; set; }

        public virtual Pacijent Pacijent { get; set; } = null!;

        public bool JeAktivna => !DatumZavrsetka.HasValue;
        public int TrajanjeUDanima => DatumZavrsetka.HasValue
            ? (DatumZavrsetka.Value - DatumPocetka).Days
            : (DateTime.Now - DatumPocetka).Days;
    }
}

