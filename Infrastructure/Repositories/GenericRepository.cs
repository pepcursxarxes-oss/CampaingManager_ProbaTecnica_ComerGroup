using Infrastructure.Persistence;
using Application.Interfaces;

namespace Infrastructure.Repositories
{
    public sealed class GenericRepository<T>(AppDbContext context) : IGenericRepository<T> where T : class
    {
        public Task AddAsync(T entity, CancellationToken cancellationToken = default)
        {
             return context.Set<T>().AddAsync(entity, cancellationToken).AsTask();
        }

        public Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return context.Set<T>().FindAsync(
            [id],
            cancellationToken).AsTask();
        }

        public Task SaveAsync(CancellationToken cancellationToken = default)
        {
            return context.SaveChangesAsync(cancellationToken);
        }
    }
}
