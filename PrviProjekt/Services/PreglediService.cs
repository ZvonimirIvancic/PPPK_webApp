using PrviProjekt.DTOs;
using PrviProjekt.Enums;
using PrviProjekt.Models;
using PrviProjekt.Repositories;

namespace PrviProjekt.Services
{
    public class PreglediService : IPreglediService 
    {
        private readonly IRepositoryFactory _repositoryFactory;
        private readonly ILogger<PreglediService> _logger;

        public PreglediService(IRepositoryFactory repositoryFactory, ILogger<PreglediService> logger)
        {
            _repositoryFactory = repositoryFactory;
            _logger = logger;
        }

        public async Task<IEnumerable<PregledDto>> GetAllPreglediAsync()
        {
            var pregledi = await _repositoryFactory.PreglediRepository.GetAllAsync();
            return pregledi.Select(MapToPregledDto);
        }

        public async Task<PregledDto?> GetPregledByIdAsync(int id)
        {
            var pregled = await _repositoryFactory.PreglediRepository.GetByIdAsync(id);
            return pregled != null ? MapToPregledDto(pregled) : null;
        }

        public async Task<IEnumerable<PregledDto>> GetPreglediByPacijentIdAsync(int pacijentId)
        {
            var pregledi = await _repositoryFactory.PreglediRepository.GetByPacijentIdAsync(pacijentId);
            return pregledi.Select(MapToPregledDto);
        }

        public async Task<IEnumerable<PregledDto>> GetPreglediByTipAsync(TipPregleda tipPregleda)
        {
            var pregledi = await _repositoryFactory.PreglediRepository.GetByTipPregledaAsync(tipPregleda);
            return pregledi.Select(MapToPregledDto);
        }

        public async Task<IEnumerable<PregledDto>> GetPreglediByDateRangeAsync(DateTime fromDate, DateTime toDate)
        {
            var pregledi = await _repositoryFactory.PreglediRepository.GetByDateRangeAsync(fromDate, toDate);
            return pregledi.Select(MapToPregledDto);
        }

        public async Task<IEnumerable<PregledDto>> GetTodaysPreglediAsync()
        {
            var pregledi = await _repositoryFactory.PreglediRepository.GetTodaysPreglediAsync();
            return pregledi.Select(MapToPregledDto);
        }

        public async Task<IEnumerable<PregledDto>> GetUpcomingPreglediAsync(int days = 7)
        {
            var pregledi = await _repositoryFactory.PreglediRepository.GetUpcomingPreglediAsync(days);
            return pregledi.Select(MapToPregledDto);
        }

        public async Task<PregledDto> CreatePregledAsync(CreatePregledDto createDto)
        {
            // Validate that patient exists
            var pacijent = await _repositoryFactory.PacijentiRepository.GetByIdAsync(createDto.PacijentId);
            if (pacijent == null)
            {
                throw new ArgumentException("Pacijent not found", nameof(createDto.PacijentId));
            }

            // Check if time slot is available
            var timeAvailable = await IsPregledTimeAvailableAsync(createDto.DatumPregleda, createDto.VrijemePregleda);
            if (!timeAvailable)
            {
                throw new InvalidOperationException("Pregled time slot is not available");
            }

            var pregled = new Pregled
            {
                PacijentId = createDto.PacijentId,
                TipPregleda = createDto.TipPregleda,
                DatumPregleda = createDto.DatumPregleda,
                VrijemePregleda = createDto.VrijemePregleda,
                Opis = createDto.Opis,
                Nalaz = createDto.Nalaz
            };

            var createdPregled = await _repositoryFactory.PreglediRepository.AddAsync(pregled);
            _logger.LogInformation($"Created pregled for patient {createDto.PacijentId}: {createDto.TipPregleda} on {createDto.DatumPregleda:yyyy-MM-dd} at {createDto.VrijemePregleda:hh\\:mm}");

            return MapToPregledDto(createdPregled);
        }

        public async Task<PregledDto> UpdatePregledAsync(int id, UpdatePregledDto updateDto)
        {
            var pregled = await _repositoryFactory.PreglediRepository.GetByIdAsync(id);
            if (pregled == null)
            {
                throw new ArgumentException("Pregled not found", nameof(id));
            }

            // Check if new time slot is available (excluding current pregled)
            var timeAvailable = await IsPregledTimeAvailableAsync(updateDto.DatumPregleda, updateDto.VrijemePregleda, id);
            if (!timeAvailable)
            {
                throw new InvalidOperationException("Pregled time slot is not available");
            }

            pregled.TipPregleda = updateDto.TipPregleda;
            pregled.DatumPregleda = updateDto.DatumPregleda;
            pregled.VrijemePregleda = updateDto.VrijemePregleda;
            pregled.Opis = updateDto.Opis;
            pregled.Nalaz = updateDto.Nalaz;

            var updatedPregled = await _repositoryFactory.PreglediRepository.UpdateAsync(pregled);
            _logger.LogInformation($"Updated pregled {id}: {updateDto.TipPregleda} on {updateDto.DatumPregleda:yyyy-MM-dd}");

            return MapToPregledDto(updatedPregled);
        }

        public async Task DeletePregledAsync(int id)
        {
            // Get pregled to check for associated data
            var pregled = await _repositoryFactory.PreglediRepository.GetWithSlikeAsync(id);
            if (pregled == null)
            {
                throw new ArgumentException("Pregled not found", nameof(id));
            }

            // Check if pregled has associated files/images that need to be handled
            if (pregled.Slike.Any())
            {
                _logger.LogWarning($"Deleting pregled {id} that has {pregled.Slike.Count} associated images");
                // Note: Images will be cascade deleted due to FK constraint
            }

            await _repositoryFactory.PreglediRepository.DeleteAsync(id);
            _logger.LogInformation($"Deleted pregled {id}");
        }

        public async Task<PregledDto?> GetPregledWithSlikeAsync(int id)
        {
            var pregled = await _repositoryFactory.PreglediRepository.GetWithSlikeAsync(id);
            return pregled != null ? MapToPregledDto(pregled) : null;
        }

        public async Task<Dictionary<TipPregleda, int>> GetPregledStatisticsAsync()
        {
            var pregledi = await _repositoryFactory.PreglediRepository.GetAllAsync();
            return pregledi
                .GroupBy(p => p.TipPregleda)
                .ToDictionary(g => g.Key, g => g.Count());
        }

        public async Task<IEnumerable<PregledDto>> GetRecentPreglediAsync(int count = 10)
        {
            var pregledi = await _repositoryFactory.PreglediRepository.GetAllAsync();
            return pregledi
                .OrderByDescending(p => p.DatumPregleda)
                .ThenByDescending(p => p.VrijemePregleda)
                .Take(count)
                .Select(MapToPregledDto);
        }

        public async Task<bool> IsPregledTimeAvailableAsync(DateTime datum, TimeSpan vrijeme, int? excludeId = null)
        {
            var existingPregledi = await _repositoryFactory.PreglediRepository.GetByDateRangeAsync(datum, datum);

            var conflictingPregledi = existingPregledi.Where(p =>
                p.DatumPregleda.Date == datum.Date &&
                Math.Abs((p.VrijemePregleda - vrijeme).TotalMinutes) < 30 && // 30-minute buffer
                (!excludeId.HasValue || p.Id != excludeId.Value));

            return !conflictingPregledi.Any();
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
                BrojSlika = pregled.Slike?.Count ?? 0,
            };
        }
    }
}
