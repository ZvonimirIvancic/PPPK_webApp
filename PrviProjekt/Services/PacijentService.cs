using PrviProjekt.DTOs;
using PrviProjekt.Models;
using PrviProjekt.Repositories;

namespace PrviProjekt.Services
{
    public class PacijentService : IPacijentService
    {
        private readonly IRepositoryFactory _repositoryFactory;

        public PacijentService(IRepositoryFactory repositoryFactory)
        {
            _repositoryFactory = repositoryFactory;
        }

        public async Task<IEnumerable<PacijentDto>> GetAllPacijentiAsync()
        {
            var pacijenti = await _repositoryFactory.PacijentiRepository.GetAllAsync();
            return pacijenti.Select(p => MapToPacijentDto(p));
        }

        public async Task<PacijentDto?> GetPacijentByIdAsync(int id)
        {
            var pacijent = await _repositoryFactory.PacijentiRepository.GetByIdAsync(id);
            return pacijent != null ? MapToPacijentDto(pacijent) : null;
        }

        public async Task<PacijentDto?> GetPacijentByOibAsync(string oib)
        {
            var pacijent = await _repositoryFactory.PacijentiRepository.GetByOibAsync(oib);
            return pacijent != null ? MapToPacijentDto(pacijent) : null;
        }

        public async Task<IEnumerable<PacijentDto>> SearchPacijentiAsync(string searchTerm)
        {
            var pacijenti = await _repositoryFactory.PacijentiRepository.SearchByPrezimeOrOibAsync(searchTerm);
            return pacijenti.Select(p => MapToPacijentDto(p));
        }

        public async Task<PacijentDto> CreatePacijentAsync(CreatePacijentDto createDto)
        {
            // Provjeri da OIB ne postoji
            if (await _repositoryFactory.PacijentiRepository.OibExistsAsync(createDto.OIB))
            {
                throw new InvalidOperationException("Pacijent s tim OIB-om već postoji");
            }

            var pacijent = new Pacijent
            {
                OIB = createDto.OIB,
                Ime = createDto.Ime,
                Prezime = createDto.Prezime,
                DatumRodenja = createDto.DatumRodenja,
                Spol = createDto.Spol
            };

            var createdPacijent = await _repositoryFactory.PacijentiRepository.AddAsync(pacijent);
            return MapToPacijentDto(createdPacijent);
        }

        public async Task<PacijentDto> UpdatePacijentAsync(int id, UpdatePacijentDto updateDto)
        {
            var pacijent = await _repositoryFactory.PacijentiRepository.GetByIdAsync(id);
            if (pacijent == null)
            {
                throw new ArgumentException("Pacijent nije pronađen", nameof(id));
            }

            if (await _repositoryFactory.PacijentiRepository.OibExistsAsync(updateDto.OIB, id))
            {
                throw new InvalidOperationException("Pacijent s tim OIB-om već postoji");
            }

            pacijent.OIB = updateDto.OIB;
            pacijent.Ime = updateDto.Ime;
            pacijent.Prezime = updateDto.Prezime;
            pacijent.DatumRodenja = updateDto.DatumRodenja;
            pacijent.Spol = updateDto.Spol;

            var updatedPacijent = await _repositoryFactory.PacijentiRepository.UpdateAsync(pacijent);
            return MapToPacijentDto(updatedPacijent);
        }

        public async Task DeletePacijentAsync(int id)
        {
            await _repositoryFactory.PacijentiRepository.DeleteAsync(id);
        }

        public async Task<MedicinskiKartonDto?> GetMedicinskiKartonAsync(int pacijentId)
        {
            var pacijent = await _repositoryFactory.PacijentiRepository.GetCompletePatientDataAsync(pacijentId);
            if (pacijent == null) return null;

            return new MedicinskiKartonDto
            {
                Pacijent = MapToPacijentDto(pacijent),
                MedicinskaDokumentacija = pacijent.MedicinskaDokumentacija.Select(md => MapToMedicinskaDokumentacijaDto(md)).ToList(),
                Pregledi = pacijent.Pregledi.Select(p => MapToPregledDto(p)).ToList(),
                Recepti = pacijent.Recepti.Select(r => MapToReceptDto(r)).ToList()
            };
        }

        public async Task<bool> OibExistsAsync(string oib, int? excludeId = null)
        {
            return await _repositoryFactory.PacijentiRepository.OibExistsAsync(oib, excludeId);
        }

        private static PacijentDto MapToPacijentDto(Pacijent pacijent)
        {
            return new PacijentDto
            {
                Id = pacijent.Id,
                OIB = pacijent.OIB,
                Ime = pacijent.Ime,
                Prezime = pacijent.Prezime,
                PunoIme = pacijent.PunoIme,
                DatumRodenja = pacijent.DatumRodenja,
                Spol = pacijent.Spol,
                Godine = pacijent.Godine,
                CreatedAt = pacijent.CreatedAt,
                UpdatedAt = pacijent.UpdatedAt
            };
        }

        private static MedicinskaDokumentacijaDto MapToMedicinskaDokumentacijaDto(MedicinskaDokumentacija md)
        {
            return new MedicinskaDokumentacijaDto
            {
                Id = md.Id,
                PacijentId = md.PacijentId,
                NazivBolesti = md.NazivBolesti,
                DatumPocetka = md.DatumPocetka,
                DatumZavrsetka = md.DatumZavrsetka,
                Opis = md.Opis,
                JeAktivna = md.JeAktivna,
                TrajanjeUDanima = md.TrajanjeUDanima
            };
        }

        private static PregledDto MapToPregledDto(Pregled pregled)
        {
            return new PregledDto
            {
                Id = pregled.Id,
                PacijentId = pregled.PacijentId,
                TipPregleda = pregled.TipPregleda.ToString(),
                TipPregledaOpis = pregled.TipPregledaOpis,
                DatumPregleda = pregled.DatumPregleda,
                VrijemePregleda = pregled.VrijemePregleda,
                DatumVrijemePregleda = pregled.DatumVrijemePregleda,
                Opis = pregled.Opis,
                Nalaz = pregled.Nalaz,
                BrojSlika = pregled.Slike.Count
            };
        }

        private static ReceptDto MapToReceptDto(Recept recept)
        {
            return new ReceptDto
            {
                Id = recept.Id,
                PacijentId = recept.PacijentId,
                PregledId = recept.PregledId,
                NazivLijeka = recept.NazivLijeka,
                Doza = recept.Doza,
                Upute = recept.Upute,
                DatumIzdavanja = recept.DatumIzdavanja,
                DatumVazenja = recept.DatumVazenja,
                JeVazeci = recept.JeVazeci,
                DanaDoIsteka = recept.DanaDoIsteka
            };
        }
    }
}
