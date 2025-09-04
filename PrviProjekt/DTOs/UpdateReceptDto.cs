using System.ComponentModel.DataAnnotations;

namespace PrviProjekt.DTOs
{
    public class UpdateReceptDto
    {
        [Required(ErrorMessage = "Naziv lijeka je obavezan")]
        [StringLength(200, ErrorMessage = "Naziv lijeka može imati maksimalno 200 znakova")]
        public string NazivLijeka { get; set; } = string.Empty;

        [Required(ErrorMessage = "Doza je obavezna")]
        [StringLength(100, ErrorMessage = "Doza može imati maksimalno 100 znakova")]
        public string Doza { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Upute mogu imati maksimalno 1000 znakova")]
        public string? Upute { get; set; }

        [Required(ErrorMessage = "Datum izdavanja je obavezan")]
        public DateTime DatumIzdavanja { get; set; }

        public DateTime? DatumVazenja { get; set; }
    }
}
