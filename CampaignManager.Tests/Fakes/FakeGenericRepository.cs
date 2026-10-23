using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Application.Interfaces;

namespace CampaignManager.Tests.Fakes;

public class FakeGenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : class
{
    private readonly List<TEntity> _items = [];

    public Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var item = _items.FirstOrDefault(entity =>
        {
            var prop = typeof(TEntity).GetProperty("Id", BindingFlags.Public | BindingFlags.Instance);
            if (prop != null)
            {
                var val = prop.GetValue(entity);
                if (val is Guid g && g == id) return true;
            }
            return false;
        });

        return Task.FromResult(item);
    }

    public Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<TEntity> list = _items.ToList();
        return Task.FromResult(list);
    }

    public Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        _items.Add(entity);
        return Task.CompletedTask;
    }

    public void Update(TEntity entity)
    {
        // No-op for in-memory object reference
    }

    public void Remove(TEntity entity)
    {
        _items.Remove(entity);
    }
}
