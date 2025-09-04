using PrviProjekt.Repositories;
using System.Text;

namespace PrviProjekt.Services
{
    public class ExportService : IExportService
    {
        private readonly IRepositoryFactory _repositoryFactory;
        private readonly ILogger<ExportService> _logger;

        public ExportService(IRepositoryFactory repositoryFactory, ILogger<ExportService> logger)
        {
            _repositoryFactory = repositoryFactory;
            _logger = logger;
        }

        public async Task<string> ExportPacijentiToCSVAsync()
        {
            try
            {
                var pacijenti = await _repositoryFactory.PacijentiRepository.GetAllAsync();

                var csv = new StringBuilder();
                csv.AppendLine("OIB,Ime,Prezime,Datum rođenja,Spol,Godine,Datum registracije,Broj pregleda,Zadnji pregled,Broj recepta");

                foreach (var pacijent in pacijenti.OrderBy(p => p.Prezime).ThenBy(p => p.Ime))
                {
                    var pregledi = await _repositoryFactory.PreglediRepository.GetByPacijentIdAsync(pacijent.Id);
                    var recepti = await _repositoryFactory.ReceptiRepository.GetByPacijentIdAsync(pacijent.Id);

                    var zadnjiPregled = pregledi.OrderByDescending(p => p.DatumPregleda).FirstOrDefault();

                    csv.AppendLine($"{pacijent.OIB}," +
                                  $"{EscapeCsvField(pacijent.Ime)}," +
                                  $"{EscapeCsvField(pacijent.Prezime)}," +
                                  $"{pacijent.DatumRodenja:yyyy-MM-dd}," +
                                  $"{(pacijent.Spol == "M" ? "Muški" : "Ženski")}," +
                                  $"{pacijent.Godine}," +
                                  $"{pacijent.CreatedAt:yyyy-MM-dd}," +
                                  $"{pregledi.Count()}," +
                                  $"{(zadnjiPregled?.DatumPregleda.ToString("yyyy-MM-dd") ?? "")}," +
                                  $"{recepti.Count()}");
                }

                _logger.LogInformation($"Exported {pacijenti.Count()} patients to CSV");
                return csv.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting patients to CSV");
                throw;
            }
        }

        public async Task<byte[]> ExportPacijentiToPDFAsync()
        {
            throw new NotImplementedException("PDF export functionality is not yet implemented");
        }

        public async Task<string> ExportPreglediToCSVAsync(int? pacijentId = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                var pregledi = await _repositoryFactory.PreglediRepository.GetAllAsync();

                if (pacijentId.HasValue)
                    pregledi = pregledi.Where(p => p.PacijentId == pacijentId.Value);

                if (fromDate.HasValue)
                    pregledi = pregledi.Where(p => p.DatumPregleda >= fromDate.Value);

                if (toDate.HasValue)
                    pregledi = pregledi.Where(p => p.DatumPregleda <= toDate.Value);

                var csv = new StringBuilder();
                csv.AppendLine("Pacijent OIB,Pacijent,Tip pregleda,Opis tipa,Datum pregleda,Vrijeme,Opis,Nalaz,Broj slika");

                foreach (var pregled in pregledi.OrderByDescending(p => p.DatumPregleda))
                {
                    var slike = await _repositoryFactory.SlikeRepository.GetByPregledIdAsync(pregled.Id);

                    csv.AppendLine($"{pregled.Pacijent.OIB}," +
                                  $"{EscapeCsvField(pregled.Pacijent.PunoIme)}," +
                                  $"{pregled.TipPregleda}," +
                                  $"{EscapeCsvField(pregled.TipPregledaOpis)}," +
                                  $"{pregled.DatumPregleda:yyyy-MM-dd}," +
                                  $"{pregled.VrijemePregleda:hh\\:mm}," +
                                  $"{EscapeCsvField(pregled.Opis ?? "")}," +
                                  $"{EscapeCsvField(pregled.Nalaz ?? "")}," +
                                  $"{slike.Count()}");
                }

                _logger.LogInformation($"Exported {pregledi.Count()} pregledi to CSV");
                return csv.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting pregledi to CSV");
                throw;
            }
        }

        public async Task<string> ExportReceptiToCSVAsync(int? pacijentId = null, bool? activeOnly = null)
        {
            try
            {
                var recepti = await _repositoryFactory.ReceptiRepository.GetAllAsync();

                if (pacijentId.HasValue)
                    recepti = recepti.Where(r => r.PacijentId == pacijentId.Value);

                if (activeOnly.HasValue && activeOnly.Value)
                    recepti = recepti.Where(r => r.JeVazeci);

                var csv = new StringBuilder();
                csv.AppendLine("Pacijent OIB,Pacijent,Naziv lijeka,Doza,Upute,Datum izdavanja,Datum važenja,Važeći,Dana do isteka");

                foreach (var recept in recepti.OrderByDescending(r => r.DatumIzdavanja))
                {
                    csv.AppendLine($"{recept.Pacijent.OIB}," +
                                  $"{EscapeCsvField(recept.Pacijent.PunoIme)}," +
                                  $"{EscapeCsvField(recept.NazivLijeka)}," +
                                  $"{EscapeCsvField(recept.Doza)}," +
                                  $"{EscapeCsvField(recept.Upute ?? "")}," +
                                  $"{recept.DatumIzdavanja:yyyy-MM-dd}," +
                                  $"{(recept.DatumVazenja?.ToString("yyyy-MM-dd") ?? "")}," +
                                  $"{(recept.JeVazeci ? "Da" : "Ne")}," +
                                  $"{(recept.DanaDoIsteka == int.MaxValue ? "Neograničeno" : recept.DanaDoIsteka.ToString())}");
                }

                _logger.LogInformation($"Exported {recepti.Count()} recepti to CSV");
                return csv.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting recepti to CSV");
                throw;
            }
        }

        public async Task<string> ExportMedicinskaDokumentacijaToCSVAsync(int? pacijentId = null, bool? activeOnly = null)
        {
            try
            {
                var dokumentacija = await _repositoryFactory.MedicinskaDokumentacijaRepository.GetAllAsync();

                if (pacijentId.HasValue)
                    dokumentacija = dokumentacija.Where(md => md.PacijentId == pacijentId.Value);

                if (activeOnly.HasValue && activeOnly.Value)
                    dokumentacija = dokumentacija.Where(md => md.JeAktivna);

                var csv = new StringBuilder();
                csv.AppendLine("Pacijent OIB,Pacijent,Naziv bolesti,Datum početka,Datum završetka,Aktivna,Trajanje (dani),Opis");

                foreach (var md in dokumentacija.OrderByDescending(md => md.DatumPocetka))
                {
                    csv.AppendLine($"{md.Pacijent.OIB}," +
                                  $"{EscapeCsvField(md.Pacijent.PunoIme)}," +
                                  $"{EscapeCsvField(md.NazivBolesti)}," +
                                  $"{md.DatumPocetka:yyyy-MM-dd}," +
                                  $"{(md.DatumZavrsetka?.ToString("yyyy-MM-dd") ?? "")}," +
                                  $"{(md.JeAktivna ? "Da" : "Ne")}," +
                                  $"{md.TrajanjeUDanima}," +
                                  $"{EscapeCsvField(md.Opis ?? "")}");
                }

                _logger.LogInformation($"Exported {dokumentacija.Count()} medicinska dokumentacija records to CSV");
                return csv.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting medicinska dokumentacija to CSV");
                throw;
            }
        }

        public async Task<string> ExportSummaryReportToCSVAsync()
        {
            try
            {
                var pacijenti = await _repositoryFactory.PacijentiRepository.GetAllAsync();
                var pregledi = await _repositoryFactory.PreglediRepository.GetAllAsync();
                var recepti = await _repositoryFactory.ReceptiRepository.GetAllAsync();
                var slike = await _repositoryFactory.SlikeRepository.GetAllAsync();
                var medDok = await _repositoryFactory.MedicinskaDokumentacijaRepository.GetAllAsync();

                var csv = new StringBuilder();
                csv.AppendLine("Kategorija,Ukupno,Dodatne informacije");

                csv.AppendLine($"Pacijenti,{pacijenti.Count()},Aktivni u sustavu");
                csv.AppendLine($"Pregledi,{pregledi.Count()},Ukupno obavljenih pregleda");
                csv.AppendLine($"Recepti,{recepti.Count()},Ukupno izdanih recepta");
                csv.AppendLine($"Slike,{slike.Count()},Ukupno uploadanih slika");
                csv.AppendLine($"Medicinska dokumentacija,{medDok.Count()},Ukupno evidencija bolesti");

                var aktivneBolesti = medDok.Count(md => md.JeAktivna);
                csv.AppendLine($"Aktivne bolesti,{aktivneBolesti},Trenutno aktivne bolesti");

                var validniRecepti = recepti.Count(r => r.JeVazeci);
                csv.AppendLine($"Važeći recepti,{validniRecepti},Trenutno važeći recepti");

                var totalStorage = await _repositoryFactory.SlikeRepository.GetTotalFileSizeAsync();
                csv.AppendLine($"Ukupno prostora (MB),{totalStorage / 1024.0 / 1024.0:F2},Zauzeto prostora na disku");

                _logger.LogInformation("Generated summary report CSV");
                return csv.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating summary report CSV");
                throw;
            }
        }

        private static string EscapeCsvField(string field)
        {
            if (string.IsNullOrEmpty(field))
                return "";

            if (field.Contains(',') || field.Contains('\n') || field.Contains('"'))
            {

                field = field.Replace("\"", "\"\"");
                return $"\"{field}\"";
            }

            return field;
        }
    }
}

