using System.Linq.Expressions;

using Azure.Data.Tables;
using Net.Shared.Persistence.Abstractions.Interfaces.Entities.Catalogs;
using Net.Shared.Persistence.Abstractions.Interfaces.Repositories;
using Net.Shared.Persistence.Abstractions.Models.Contexts;
using Net.Shared.Persistence.Contexts;

using static Net.Shared.Persistence.Abstractions.Constants.Enums;

namespace Net.Shared.Persistence.Repositories.AzureTable;

public sealed class AzureTableProcessRepository(AzureTableContext context) : IPersistenceProcessRepository<ITableEntity>
{
    private readonly AzureTableContext _context = context;

    Task<T[]> IPersistenceProcessRepository<ITableEntity>.GetProcessSteps<T>(CancellationToken cToken) =>
        _context.FindMany<T>(new(), cToken);
    Task<T[]> IPersistenceProcessRepository<ITableEntity>.GetProcessSteps<T>(Expression<Func<T, bool>> filter, CancellationToken cToken)
    {
        throw new NotImplementedException();
    }
    Task<T[]> IPersistenceProcessRepository<ITableEntity>.GetProcessableData<T>(Guid correlationId, IPersistentProcessStep step, int limit, CancellationToken cToken)
    {
        var updated = DateTime.UtcNow;

        var options = new PersistenceUpdateOptions<T>(Update, nameof(ITableEntity.RowKey))
        {
            QueryOptions = new()
            {
                Filter = x =>
                    x.CorrelationId == null || x.CorrelationId == correlationId
                    && x.StepId == step.Id
                    && x.StatusId == (int)ProcessStatuses.Ready,
                Take = limit,
                OrderBy = x => x.Updated
            }
        };

        return _context.Update(options, cToken);

        void Update(T x)
        {
            x.CorrelationId = correlationId;
            x.StatusId = (int)ProcessStatuses.Processing;
            x.Attempt++;
            x.Updated = updated;
        }
    }
    Task<T[]> IPersistenceProcessRepository<ITableEntity>.GetProcessableData<T>(Guid correlationId, IPersistentProcessStep step, int limit, Expression<Func<T, bool>> filter, CancellationToken cToken)
    {
        throw new NotImplementedException();
    }
    async Task<T[]> IPersistenceProcessRepository<ITableEntity>.GetUnprocessedData<T>(Guid correlationId, IPersistentProcessStep step, int limit, DateTime updateTime, int maxAttempts, CancellationToken cToken)
    {
        var updated = DateTime.UtcNow;

        var options = new PersistenceUpdateOptions<T>(Update, nameof(ITableEntity.RowKey))
        {
            QueryOptions = new()
            {
                Filter = x =>
                    x.CorrelationId == correlationId
                    && x.StepId == step.Id
                    && ((x.StatusId == (int)ProcessStatuses.Processing && x.Updated < updateTime) || x.StatusId == (int)ProcessStatuses.Error)
                    && x.Attempt <= maxAttempts,
                OrderBy = x => x.Updated,
                Take = limit
            }
        };

        return await _context.Update(options, cToken);

        void Update(T x)
        {
            x.StatusId = (int)ProcessStatuses.Processing;
            x.Attempt++;
            x.Updated = updated;
        }
    }
    Task<T[]> IPersistenceProcessRepository<ITableEntity>.GetUnprocessedData<T>(Guid correlationId, IPersistentProcessStep step, int limit, DateTime updateTime, int maxAttempts, Expression<Func<T, bool>> filter, CancellationToken cToken)
    {
        throw new NotImplementedException();
    }
    async Task IPersistenceProcessRepository<ITableEntity>.SetProcessedData<T>(Guid correlationId, IPersistentProcessStep currentStep, IPersistentProcessStep? nextStep, IEnumerable<T> data, CancellationToken cToken)
    {
        var updated = DateTime.UtcNow;

        var options = new PersistenceUpdateOptions<T>(Update, data, nameof(ITableEntity.RowKey))
        {
            QueryOptions = new()
            {
                Filter = x =>
                x.CorrelationId == correlationId
                && x.StepId == currentStep.Id
                && x.StatusId == (int)ProcessStatuses.Processing
            }
        };

        await _context.Update(options, cToken);

        void Update(T x)
        {
            x.Updated = updated;

            if (x.StatusId != (int)ProcessStatuses.Error)
            {
                x.Error = null;

                switch (nextStep)
                {
                    case not null:
                        x.StatusId = (int)ProcessStatuses.Ready;
                        x.StepId = nextStep.Id;
                        break;
                    default:
                        x.StatusId = (int)ProcessStatuses.Completed;
                        break;
                }
            }
        }
    }
}
