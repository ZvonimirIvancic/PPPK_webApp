using Microsoft.EntityFrameworkCore.Storage;
using PrviProjekt.Data;

namespace PrviProjekt.Repositories
{
    public class RepositoryFactory : IRepositoryFactory, IDisposable
    {
        private readonly MedicinskiDbContext _context;
        private readonly Lazy<IPacijentRepository> _pacijentiRepository;
        private readonly Lazy<IMedicinskaDokumentacijaRepository> _medicinskaDokumentacijaRepository;
        private readonly Lazy<IPreglediRepository> _preglediRepository;
        private readonly Lazy<ISlikeRepository> _slikeRepository;
        private readonly Lazy<IReceptiRepository> _receptiRepository;
        private IDbContextTransaction? _transaction;

        public RepositoryFactory(MedicinskiDbContext context)
        {
            _context = context;
            _pacijentiRepository = new Lazy<IPacijentRepository>(() => new PacijentRepository(_context));
            _medicinskaDokumentacijaRepository = new Lazy<IMedicinskaDokumentacijaRepository>(() => new MedicinskaDokumentacijaRepository(_context));
            _preglediRepository = new Lazy<IPreglediRepository>(() => new PreglediRepository(_context));
            _slikeRepository = new Lazy<ISlikeRepository>(() => new SlikeRepository(_context));
            _receptiRepository = new Lazy<IReceptiRepository>(() => new ReceptiRepository(_context));
        }

        public IPacijentRepository PacijentiRepository => _pacijentiRepository.Value;
        public IMedicinskaDokumentacijaRepository MedicinskaDokumentacijaRepository => _medicinskaDokumentacijaRepository.Value;
        public IPreglediRepository PreglediRepository => _preglediRepository.Value;
        public ISlikeRepository SlikeRepository => _slikeRepository.Value;
        public IReceptiRepository ReceptiRepository => _receptiRepository.Value;

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void BeginTransaction()
        {
            _transaction = _context.Database.BeginTransaction();
        }

        public async Task CommitTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
        }
    }
}
