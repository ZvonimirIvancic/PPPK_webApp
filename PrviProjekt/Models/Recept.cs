using System.ComponentModel.DataAnnotations;

namespace PrviProjekt.Models
{
    public class Recept : BaseEntity
    {
        [Required(ErrorMessage = "Pacijent ID je obavezan")]
        public int PacijentId { get; set; }

        public int? PregledId { get; set; }

        [Required(ErrorMessage = "Naziv lijeka je obavezan")]
        [StringLength(200, ErrorMessage = "Naziv lijeka može imati maksimalno 200 znakova")]
        public string NazivLijeka { get; set; } = string.Empty;

        [Required(ErrorMessage = "Doza je obavezna")]
        [StringLength(100, ErrorMessage = "Doza može imati maksimalno 100 znakova")]
        public string Doza { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Upute mogu imati maksimalno 1000 znakova")]
        public string? Upute { get; set; }

        [Required(ErrorMessage = "Datum izdavanja je obavezan")]
        [DataType(DataType.Date)]
        public DateTime DatumIzdavanja { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DatumVazenja { get; set; }
        public virtual Pacijent Pacijent { get; set; } = null!;
        public virtual Pregled? Pregled { get; set; }

        public bool JeVazeci
        {
            get
            {
                if (!DatumVazenja.HasValue) return true;
                return DatumVazenja.Value >= DateTime.Today;
            }
        }

        public int DanaDoIsteka
        {
            get
            {
                if (!DatumVazenja.HasValue) return int.MaxValue;
                return (DatumVazenja.Value - DateTime.Today).Days;
            }
        }
    }
}

