using PrviProjekt.Enums;
using System.ComponentModel.DataAnnotations;

namespace PrviProjekt.DTOs
{
    public class CreatePregledDto
    {
        [Required(ErrorMessage = "Pacijent je obavezan")]
        public int PacijentId { get; set; }

        [Required(ErrorMessage = "Tip pregleda je obavezan")]
        public TipPregleda TipPregleda { get; set; }

        [Required(ErrorMessage = "Datum pregleda je obavezan")]
        public DateTime DatumPregleda { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Vrijeme pregleda je obavezno")]
        public TimeSpan VrijemePregleda { get; set; } = new TimeSpan(9, 0, 0);

        [StringLength(1000, ErrorMessage = "Opis može imati maksimalno 1000 znakova")]
        public string? Opis { get; set; }

        [StringLength(2000, ErrorMessage = "Nalaz može imati maksimalno 2000 znakova")]
        public string? Nalaz { get; set; }
    }
}
