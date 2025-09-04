using System.ComponentModel.DataAnnotations;

namespace PrviProjekt.Models
{
    public class Slika
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Pregled ID je obavezan")]
        public int PregledId { get; set; }

        [Required(ErrorMessage = "Naziv datoteke je obavezan")]
        [StringLength(255, ErrorMessage = "Naziv datoteke može imati maksimalno 255 znakova")]
        public string NazivDatoteke { get; set; } = string.Empty;

        [Required(ErrorMessage = "Putanja je obavezna")]
        [StringLength(500, ErrorMessage = "Putanja može imati maksimalno 500 znakova")]
        public string Putanja { get; set; } = string.Empty;

        [StringLength(50, ErrorMessage = "Tip datoteke može imati maksimalno 50 znakova")]
        public string? TipDatoteke { get; set; }

        public long? VelicinaDatoteke { get; set; }

        public DateTime DatumUpload { get; set; } = DateTime.UtcNow;

        [StringLength(500, ErrorMessage = "Opis može imati maksimalno 500 znakova")]
        public string? Opis { get; set; }
        public virtual Pregled Pregled { get; set; } = null!;

        public string VelicinaFormatirana
        {
            get
            {
                if (!VelicinaDatoteke.HasValue) return "Nepoznato";

                long bytes = VelicinaDatoteke.Value;
                string[] suffixes = { "B", "KB", "MB", "GB" };
                int suffixIndex = 0;
                double size = bytes;

                while (size >= 1024 && suffixIndex < suffixes.Length - 1)
                {
                    size /= 1024;
                    suffixIndex++;
                }

                return $"{size:F2} {suffixes[suffixIndex]}";
            }
        }
    }
}

