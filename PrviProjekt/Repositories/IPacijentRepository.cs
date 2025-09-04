using PrviProjekt.Models;

namespace PrviProjekt.Repositories
{
    public interface IPacijentRepository : IRepository<Pacijent>
    {
        Task<IEnumerable<Pacijent>> SearchByPrezimeOrOibAsync(string searchTerm);
        Task<Pacijent?> GetByOibAsync(string oib);
        Task<Pacijent?> GetWithMedicinskaDokumentacijaAsync(int id);
        Task<Pacijent?> GetWithPreglediAsync(int id);
        Task<Pacijent?> GetWithReceptiAsync(int id);
        Task<Pacijent?> GetCompletePatientDataAsync(int id);
        Task<bool> OibExistsAsync(string oib, int? excludeId = null);
    }
}
