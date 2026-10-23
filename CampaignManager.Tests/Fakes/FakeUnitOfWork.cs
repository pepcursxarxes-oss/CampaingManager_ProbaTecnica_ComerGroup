using System.Threading;
using System.Threading.Tasks;
using Application.Interfaces;

namespace CampaignManager.Tests.Fakes;

public class FakeUnitOfWork : IUnitOfWork
{
    public int SaveChangesCallCount { get; private set; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SaveChangesCallCount++;
        return Task.FromResult(1);
    }
}
