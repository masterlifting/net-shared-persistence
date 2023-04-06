using Net.Shared.Persistence.Abstractions.Entities;
using Net.Shared.Persistence.Abstractions.Entities.Catalogs;

namespace Net.Shared.Persistence.Abstractions.Repositories;

public interface IPersistenceProcessRepository
{
    Task<T[]> GetSteps<T>(CancellationToken cToken) where T : class, IPersistentProcessStep;
    Task<T[]> GetProcessableData<T>(IPersistentProcessStep step, int limit, CancellationToken cToken) where T : class, IPersistentProcess;
    Task<T[]> GetUnprocessableData<T>(IPersistentProcessStep step, int limit, DateTime updateTime, int maxAttempts, CancellationToken cToken) where T : class, IPersistentProcess;
    Task SetProcessableData<T>(IPersistentProcessStep? step, IEnumerable<T> entities, CancellationToken cToken) where T : class, IPersistentProcess;
}
