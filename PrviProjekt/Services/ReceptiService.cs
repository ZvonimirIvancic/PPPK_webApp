using PrviProjekt.DTOs;
using PrviProjekt.Models;
using PrviProjekt.Repositories;

namespace PrviProjekt.Services
{
    public class ReceptiService : IReceptiService
    {
        private readonly IRepositoryFactory _repositoryFactory;
        private readonly ILogger<ReceptiService> _logger;

        public ReceptiService(IRepositoryFactory repositoryFactory, ILogger<ReceptiService> logger)
        {
            _repositoryFactory = repositoryFactory;
            _logger = logger;
        }

        public async Task<IEnumerable<ReceptDto>> GetAllReceptiAsync()
        {
            var recepti = await _repositoryFactory.ReceptiRepository.GetAllAsync();
            return recepti.Select(MapToReceptDto);
        }

        public async Task<ReceptDto?> GetReceptByIdAsync(int id)
        {
            var recept = await _repositoryFactory.ReceptiRepository.GetByIdAsync(id);
            return recept != null ? MapToReceptDto(recept) : null;
        }

        public async Task<IEnumerable<ReceptDto>> GetReceptiByPacijentIdAsync(int pacijentId)
        {
            var recepti = await _repositoryFactory.ReceptiRepository.GetByPacijentIdAsync(pacijentId);
            return recepti.Select(MapToReceptDto);
        }

        public async Task<IEnumerable<ReceptDto>> GetValidReceptiAsync()
        {
            var recepti = await _repositoryFactory.ReceptiRepository.GetValidReceptiAsync();
            return recepti.Select(MapToReceptDto);
        }

        public async Task<IEnumerable<ReceptDto>> GetExpiringReceptiAsync(int daysAhead = 30)
        {
            var recepti = await _repositoryFactory.ReceptiRepository.GetExpiringReceptiAsync(daysAhead);
            return recepti.Select(MapToReceptDto);
        }

        public async Task<IEnumerable<ReceptDto>> SearchReceptiByLijekAsync(string nazivLijeka)
        {
            var recepti = await _repositoryFactory.ReceptiRepository.GetByLijekAsync(nazivLijeka);
            return recepti.Select(MapToReceptDto);
        }

        public async Task<ReceptDto> CreateReceptAsync(CreateReceptDto createDto)
        {
            // Validate that patient exists
            var pacijent = await _repositoryFactory.PacijentiRepository.GetByIdAsync(createDto.PacijentId);
            if (pacijent == null)
            {
                throw new ArgumentException("Pacijent not found", nameof(createDto.PacijentId));
            }

            // Validate pregled if provided
            if (createDto.PregledId.HasValue)
            {
                var pregled = await _repositoryFactory.PreglediRepository.GetByIdAsync(createDto.PregledId.Value);
                if (pregled == null)
                {
                    throw new ArgumentException("Pregled not found", nameof(createDto.PregledId));
                }
            }

            var recept = new Recept
            {
                PacijentId = createDto.PacijentId,
                PregledId = createDto.PregledId,
                NazivLijeka = createDto.NazivLijeka,
                Doza = createDto.Doza,
                Upute = createDto.Upute,
                DatumIzdavanja = createDto.DatumIzdavanja,
                DatumVazenja = createDto.DatumVazenja
            };

            var createdRecept = await _repositoryFactory.ReceptiRepository.AddAsync(recept);
            _logger.LogInformation($"Created recept for patient {createDto.PacijentId}: {createDto.NazivLijeka}");

            return MapToReceptDto(createdRecept);
        }

        public async Task<ReceptDto> UpdateReceptAsync(int id, UpdateReceptDto updateDto)
        {
            var recept = await _repositoryFactory.ReceptiRepository.GetByIdAsync(id);
            if (recept == null)
            {
                throw new ArgumentException("Recept not found", nameof(id));
            }

            recept.NazivLijeka = updateDto.NazivLijeka;
            recept.Doza = updateDto.Doza;
            recept.Upute = updateDto.Upute;
            recept.DatumIzdavanja = updateDto.DatumIzdavanja;
            recept.DatumVazenja = updateDto.DatumVazenja;

            var updatedRecept = await _repositoryFactory.ReceptiRepository.UpdateAsync(recept);
            _logger.LogInformation($"Updated recept {id}: {updateDto.NazivLijeka}");

            return MapToReceptDto(updatedRecept);
        }

        public async Task DeleteReceptAsync(int id)
        {
            await _repositoryFactory.ReceptiRepository.DeleteAsync(id);
            _logger.LogInformation($"Deleted recept {id}");
        }

        public async Task<Dictionary<string, int>> GetLijekStatisticsAsync()
        {
            return await _repositoryFactory.ReceptiRepository.GetLijekStatisticsAsync();
        }

        public async Task<int> GetActiveReceptiCountAsync()
        {
            return await _repositoryFactory.ReceptiRepository.GetActiveReceptiCountAsync();
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
                DanaDoIsteka = recept.DanaDoIsteka,
            };
        }
    }
}
