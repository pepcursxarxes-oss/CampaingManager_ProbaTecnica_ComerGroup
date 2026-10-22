using Application.Interfaces;
using Infrastructure.Persistence;

namespace Infrastructure.Repositories
{
    public sealed class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        public UnitOfWork(AppDbContext context)
        {
            _context = context;
        }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        { 
            return _context.SaveChangesAsync(cancellationToken);
        }
    }
}