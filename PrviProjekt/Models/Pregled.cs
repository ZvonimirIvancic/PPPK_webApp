using System.ComponentModel.DataAnnotations;
using PrviProjekt.Enums;

namespace PrviProjekt.Models
{
    public class Pregled : BaseEntity
    {
        [Required(ErrorMessage = "Pacijent ID je obavezan")]
        public int PacijentId { get; set; }

        [Required(ErrorMessage = "Tip pregleda je obavezan")]
        public TipPregleda TipPregleda { get; set; }

        [Required(ErrorMessage = "Datum pregleda je obavezan")]
        [DataType(DataType.Date)]
        public DateTime DatumPregleda { get; set; }

        [Required(ErrorMessage = "Vrijeme pregleda je obavezno")]
        [DataType(DataType.Time)]
        public TimeSpan VrijemePregleda { get; set; }

        [StringLength(1000, ErrorMessage = "Opis može imati maksimalno 1000 znakova")]
        public string? Opis { get; set; }

        [StringLength(2000, ErrorMessage = "Nalaz može imati maksimalno 2000 znakova")]
        public string? Nalaz { get; set; }

        public virtual Pacijent Pacijent { get; set; } = null!;
        public virtual ICollection<Slika> Slike { get; set; } = new List<Slika>();
        public virtual ICollection<Recept> Recepti { get; set; } = new List<Recept>();

        public string TipPregledaOpis => GetTipPregledaOpis();
        public DateTime DatumVrijemePregleda => DatumPregleda.Date.Add(VrijemePregleda);

        private string GetTipPregledaOpis()
        {
            return TipPregleda switch
            {
                TipPregleda.GP => "Opći tjelesni pregled",
                TipPregleda.KRV => "Test krvi",
                TipPregleda.XRAY => "Rendgensko skeniranje",
                TipPregleda.CT => "CT sken",
                TipPregleda.MR => "MRI sken",
                TipPregleda.ULTRA => "Ultrazvuk",
                TipPregleda.EKG => "Elektrokardiogram",
                TipPregleda.ECHO => "Ehokardiogram",
                TipPregleda.EYE => "Pregled očiju",
                TipPregleda.DERM => "Dermatološki pregled",
                TipPregleda.DENTA => "Pregled zuba",
                TipPregleda.MAMMO => "Mamografija",
                TipPregleda.NEURO => "Neurološki pregled",
                _ => "Nepoznat tip pregleda"
            };
        }
    }
}
