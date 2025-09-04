namespace PrviProjekt.Repositories
{
    public interface IRepositoryFactory
    {
        IPacijentRepository PacijentiRepository { get; }
        IMedicinskaDokumentacijaRepository MedicinskaDokumentacijaRepository { get; }
        IPreglediRepository PreglediRepository { get; }
        ISlikeRepository SlikeRepository { get; }
        IReceptiRepository ReceptiRepository { get; }
        Task<int> SaveChangesAsync();
        void BeginTransaction();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
