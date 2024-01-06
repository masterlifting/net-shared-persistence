using Net.Shared.Persistence.Abstractions.Interfaces.Entities;
using Net.Shared.Persistence.Abstractions.Interfaces.Entities.Catalogs;

namespace Net.Shared.Persistence.Abstractions.Interfaces.Repositories;

public interface IPersistenceProcessRepository<TEntity>
{
    Task<T[]> GetProcessSteps<T>(CancellationToken cToken = default) where T : class, TEntity, IPersistentProcessStep;
    Task<T[]> GetProcessableData<T>(Guid hostId, IPersistentProcessStep step, int limit, CancellationToken cToken = default) where T : class, TEntity, IPersistentProcess;
    Task<T[]> GetUnprocessedData<T>(Guid hostId, IPersistentProcessStep step, int limit, DateTime updateTime, int maxAttempts, CancellationToken cToken = default) where T : class, TEntity, IPersistentProcess;
    Task SetProcessedData<T>(Guid hostId, IPersistentProcessStep currentStep, IPersistentProcessStep? nextStep, IEnumerable<T> data, CancellationToken cToken = default) where T : class, TEntity, IPersistentProcess;
}
