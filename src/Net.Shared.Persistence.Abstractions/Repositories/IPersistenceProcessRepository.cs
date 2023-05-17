using Net.Shared.Persistence.Abstractions.Entities;
using Net.Shared.Persistence.Abstractions.Entities.Catalogs;

namespace Net.Shared.Persistence.Abstractions.Repositories;

public interface IPersistenceProcessRepository<TEntity> where TEntity : class, IPersistent
{
    Task<T[]> GetProcessSteps<T>(CancellationToken cToken) where T : class, TEntity, IPersistentProcessStep;
    Task<T[]> GetProcessableData<T>(Guid hostId, IPersistentProcessStep step, int limit, CancellationToken cToken) where T : class, TEntity, IPersistentProcess;
    Task<T[]> GetUnprocessedData<T>(Guid hostId, IPersistentProcessStep step, int limit, DateTime updateTime, int maxAttempts, CancellationToken cToken) where T : class, TEntity, IPersistentProcess;
    Task SetProcessedData<T>(Guid hostId, IPersistentProcessStep? step, IEnumerable<T> data, CancellationToken cToken) where T : class, TEntity, IPersistentProcess;
}
