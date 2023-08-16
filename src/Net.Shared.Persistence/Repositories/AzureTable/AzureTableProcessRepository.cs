using Azure.Data.Tables;

using Net.Shared.Persistence.Abstractions.Entities.Catalogs;
using Net.Shared.Persistence.Abstractions.Repositories;
using Net.Shared.Persistence.Contexts;
using Net.Shared.Persistence.Models.Contexts;

using static Net.Shared.Persistence.Models.Constants.Enums;

namespace Net.Shared.Persistence.Repositories.AzureTable;

public sealed class AzureTableProcessRepository : IPersistenceProcessRepository<ITableEntity>
{
    private readonly AzureTableContext _context;

    public AzureTableProcessRepository(AzureTableContext context)
    {
        _context = context;
    }
    Task<T[]> IPersistenceProcessRepository<ITableEntity>.GetProcessableData<T>(Guid hostId, IPersistentProcessStep step, int limit, CancellationToken cToken)
    {
        var updated = DateTime.UtcNow;

        var updater = (T x) =>
        {
            x.HostId = hostId;
            x.StatusId = (int)ProcessStatuses.Processing;
            x.Attempt++;
            x.Updated = updated;
        };

        var options = new PersistenceUpdateOptions<T>(updater)
        {
            QueryOptions = new()
            {
                Filter = x =>
                    x.HostId == null || x.HostId == hostId
                    && x.StepId == step.Id
                    && x.StatusId == (int)ProcessStatuses.Ready,
                Take = limit
            }
        };

        return _context.Update(options, cToken);
    }

    Task<T[]> IPersistenceProcessRepository<ITableEntity>.GetProcessSteps<T>(CancellationToken cToken)
    {
        throw new NotImplementedException();
    }

    async Task<T[]> IPersistenceProcessRepository<ITableEntity>.GetUnprocessedData<T>(Guid hostId, IPersistentProcessStep step, int limit, DateTime updateTime, int maxAttempts, CancellationToken cToken)
    {
        throw new NotImplementedException();
    }

    Task IPersistenceProcessRepository<ITableEntity>.SetProcessedData<T>(Guid hostId, IPersistentProcessStep currentStep, IPersistentProcessStep? nextStep, IEnumerable<T> data, CancellationToken cToken)
    {
        throw new NotImplementedException();
    }
}
