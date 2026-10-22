namespace Application.Interfaces
{
    public interface IGenericRepository<T> where T: class 
    {
        Task AddAsync(
            T entity, CancellationToken cancellationToken = default);

        Task<T?> GetByIdAsync(
            Guid id, CancellationToken cancellationToken = default);

        Task SaveAsync(CancellationToken cancellationToken = default);
    }
}
