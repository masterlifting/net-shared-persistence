using System.Linq.Expressions;

using Net.Shared.Extensions.Expression;
using Net.Shared.Persistence.Abstractions.Interfaces.Entities;
using Net.Shared.Persistence.Abstractions.Interfaces.Entities.Catalogs;
using Net.Shared.Persistence.Abstractions.Interfaces.Repositories;
using Net.Shared.Persistence.Abstractions.Models.Contexts;
using Net.Shared.Persistence.Contexts;

using static Net.Shared.Persistence.Abstractions.Constants.Enums;

namespace Net.Shared.Persistence.Repositories.MongoDb;

public class MongoDbProcessRepository<TContext, TEntity>(TContext context) : IPersistenceProcessRepository<TEntity> 
    where TContext : MongoDbContext
    where TEntity : IPersistentNoSql, IPersistentProcess
{
    private readonly TContext _context = context;

    public Task<T[]> GetProcessableData<T>(Guid correlationId, IPersistentProcessStep step, int limit, CancellationToken cToken) where T : class, TEntity
    { 
        var updated = DateTime.UtcNow;

        var options = new PersistenceUpdateOptions<T>(Update)
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
    public Task<T[]> GetProcessableData<T>(Guid correlationId, IPersistentProcessStep step, int limit, Expression<Func<T, bool>> filter, CancellationToken cToken) where T : class, TEntity
    {
        var updated = DateTime.UtcNow;

        var options = new PersistenceUpdateOptions<T>(Update)
        {
            QueryOptions = new()
            {
                Filter = filter.Combine(x =>
                    x.CorrelationId == null || x.CorrelationId == correlationId
                    && x.StepId == step.Id
                    && x.StatusId == (int)ProcessStatuses.Ready),
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

    public Task<T[]> GetUnprocessedData<T>(Guid correlationId, IPersistentProcessStep step, int limit, DateTime updateTime, int maxAttempts, CancellationToken cToken) where T : class, TEntity
    {
        var updated = DateTime.UtcNow;

        var options = new PersistenceUpdateOptions<T>(Update)
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

        return _context.Update(options, cToken);

        void Update(T x)
        {
            x.StatusId = (int)ProcessStatuses.Processing;
            x.Attempt++;
            x.Updated = updated;
        }
    }
    public Task<T[]> GetUnprocessedData<T>(Guid correlationId, IPersistentProcessStep step, int limit, DateTime updateTime, int maxAttempts, Expression<Func<T, bool>> filter, CancellationToken cToken) where T : class, TEntity
    {
        var updated = DateTime.UtcNow;

        var options = new PersistenceUpdateOptions<T>(Update)
        {
            QueryOptions = new()
            {
                Filter = filter.Combine(x =>
                    x.CorrelationId == correlationId
                    && x.StepId == step.Id
                    && ((x.StatusId == (int)ProcessStatuses.Processing && x.Updated < updateTime) || x.StatusId == (int)ProcessStatuses.Error)
                    && x.Attempt <= maxAttempts),
                OrderBy = x => x.Updated,
                Take = limit
            }
        };

        return _context.Update(options, cToken);

        void Update(T x)
        {
            x.StatusId = (int)ProcessStatuses.Processing;
            x.Attempt++;
            x.Updated = updated;
        }
    }

    public Task SetProcessedData<T>(Guid correlationId, IPersistentProcessStep currentStep, IPersistentProcessStep? nextStep, IEnumerable<T> data, CancellationToken cToken) where T : class, TEntity
    {
        var updated = DateTime.UtcNow;

        var options = new PersistenceUpdateOptions<T>(Update, data)
        {
            QueryOptions = new()
            {
                Filter = x =>
                x.CorrelationId == correlationId
                && x.StepId == currentStep.Id
                && x.StatusId == (int)ProcessStatuses.Processing
            }
        };

        return _context.Update(options, cToken);

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
