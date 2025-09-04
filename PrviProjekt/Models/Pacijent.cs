using System.ComponentModel.DataAnnotations;

namespace PrviProjekt.Models
{
    public class Pacijent : BaseEntity
    {
        [Required(ErrorMessage = "OIB je obavezan")]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "OIB mora imati točno 11 znakova")]
        [RegularExpression(@"^\d{11}$", ErrorMessage = "OIB mora sadržavati samo brojeve")]
        public string OIB { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ime je obavezno")]
        [StringLength(100, ErrorMessage = "Ime može imati maksimalno 100 znakova")]
        public string Ime { get; set; } = string.Empty;

        [Required(ErrorMessage = "Prezime je obavezno")]
        [StringLength(100, ErrorMessage = "Prezime može imati maksimalno 100 znakova")]
        public string Prezime { get; set; } = string.Empty;

        [Required(ErrorMessage = "Datum rođenja je obavezan")]
        [DataType(DataType.Date)]
        public DateTime DatumRodenja { get; set; }

        [Required(ErrorMessage = "Spol je obavezan")]
        [RegularExpression(@"^[MŽ]$", ErrorMessage = "Spol mora biti M ili Ž")]
        public string Spol { get; set; } = string.Empty;
        public virtual ICollection<MedicinskaDokumentacija> MedicinskaDokumentacija { get; set; } = new List<MedicinskaDokumentacija>();
        public virtual ICollection<Pregled> Pregledi { get; set; } = new List<Pregled>();
        public virtual ICollection<Recept> Recepti { get; set; } = new List<Recept>();

        public string PunoIme => $"{Ime} {Prezime}";
        public int Godine => DateTime.Now.Year - DatumRodenja.Year - (DateTime.Now.DayOfYear < DatumRodenja.DayOfYear ? 1 : 0);
    }
}
