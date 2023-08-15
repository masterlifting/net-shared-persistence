using Net.Shared.Persistence.Abstractions.Contexts;
using Net.Shared.Persistence.Abstractions.Entities;
using Net.Shared.Persistence.Abstractions.Entities.Catalogs;
using Net.Shared.Persistence.Abstractions.Repositories;
using Net.Shared.Persistence.Abstractions.Repositories.NoSql;

namespace Net.Shared.Persistence.Repositories.AzureTable
{
    public sealed class AzureTableProcessRepository : IPersistenceNoSqlProcessRepository
    {
        public IPersistenceNoSqlContext Context { get; }

        Task<T[]> IPersistenceProcessRepository<IPersistentNoSql>.GetProcessableData<T>(Guid hostId, IPersistentProcessStep step, int limit, CancellationToken cToken)
        {
            throw new NotImplementedException();
        }

        Task<T[]> IPersistenceProcessRepository<IPersistentNoSql>.GetProcessSteps<T>(CancellationToken cToken)
        {
            throw new NotImplementedException();
        }

        Task<T[]> IPersistenceProcessRepository<IPersistentNoSql>.GetUnprocessedData<T>(Guid hostId, IPersistentProcessStep step, int limit, DateTime updateTime, int maxAttempts, CancellationToken cToken)
        {
            throw new NotImplementedException();
        }

        Task IPersistenceProcessRepository<IPersistentNoSql>.SetProcessedData<T>(Guid hostId, IPersistentProcessStep currentStep, IPersistentProcessStep? nextStep, IEnumerable<T> data, CancellationToken cToken)
        {
            throw new NotImplementedException();
        }
    }
}
