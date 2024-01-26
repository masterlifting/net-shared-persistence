using System.Linq.Expressions;

using Microsoft.EntityFrameworkCore;

using Net.Shared.Persistence.Abstractions.Interfaces.Contexts;
using Net.Shared.Persistence.Abstractions.Interfaces.Entities;
using Net.Shared.Persistence.Abstractions.Interfaces.Entities.Catalogs;
using Net.Shared.Persistence.Abstractions.Interfaces.Repositories;
using Net.Shared.Persistence.Abstractions.Interfaces.Repositories.Sql;
using Net.Shared.Persistence.Abstractions.Models.Contexts;
using Net.Shared.Persistence.Contexts;

using static Net.Shared.Persistence.Abstractions.Constants.Enums;

namespace Net.Shared.Persistence.Repositories.PostgreSql;

public sealed class PostgreSqlProcessRepository(PostgreSqlContext context) : IPersistenceSqlProcessRepository
{
    private readonly PostgreSqlContext _context = context;
    public IPersistenceSqlContext Context { get; } = context;

    public Task<T[]> GetProcessSteps<T>(CancellationToken cToken) where T : class, IPersistentSql, IPersistentProcessStep =>
        _context.FindMany<T>(new(), cToken);
    Task<T[]> IPersistenceProcessRepository<IPersistentSql>.GetProcessSteps<T>(Expression<Func<T, bool>> filter, CancellationToken cToken)
    {
        throw new NotImplementedException();
    }
    public async Task<T[]> GetProcessableData<T>(Guid correlationId, IPersistentProcessStep step, int limit, CancellationToken cToken) where T : class, IPersistentSql, IPersistentProcess
    {
        var updated = DateTime.UtcNow;

        var updatedCount = await _context.GetQuery<T>()
            .Where(x =>
                x.CorrelationId == null || x.CorrelationId == correlationId
                && x.StepId == step.Id
                && x.StatusId == (int)ProcessStatuses.Ready)
            .Take(limit)
            .OrderBy(x => x.Updated)
            .ExecuteUpdateAsync(x => x
                .SetProperty(y => y.CorrelationId, correlationId)
                .SetProperty(y => y.StatusId, (int)ProcessStatuses.Processing)
                .SetProperty(y => y.Attempt, y => y.Attempt + 1)
                .SetProperty(y => y.Updated, updated)
            , cToken);

        var result = await _context.GetQuery<T>()
            .Where(x =>
                x.CorrelationId == correlationId
                && x.StepId == step.Id
                && x.StatusId == (int)ProcessStatuses.Processing
                && x.Updated == updated)
            .ToArrayAsync(cToken);

        return result;
    }
    Task<T[]> IPersistenceProcessRepository<IPersistentSql>.GetProcessableData<T>(Guid correlationId, IPersistentProcessStep step, int limit, Expression<Func<T, bool>> filter, CancellationToken cToken)
    {
        throw new NotImplementedException();
    }
    public async Task<T[]> GetUnprocessedData<T>(Guid correlationId, IPersistentProcessStep step, int limit, DateTime updateTime, int maxAttempts, CancellationToken cToken) where T : class, IPersistentSql, IPersistentProcess
    {
        var updated = DateTime.UtcNow;

        var updatedCount = await _context.GetQuery<T>()
            .Where(x =>
                x.CorrelationId == correlationId
                && x.StepId == step.Id
                && ((x.StatusId == (int)ProcessStatuses.Processing && x.Updated < updateTime) || x.StatusId == (int)ProcessStatuses.Error)
                && x.Attempt <= maxAttempts)
            .Take(limit)
            .OrderBy(x => x.Updated)
            .ExecuteUpdateAsync(x => x
                .SetProperty(y => y.StatusId, (int)ProcessStatuses.Processing)
                .SetProperty(y => y.Attempt, y => y.Attempt + 1)
                .SetProperty(y => y.Updated, updated)
            , cToken);

        var result = await _context.GetQuery<T>()
            .Where(x =>
                x.CorrelationId == correlationId
                && x.StepId == step.Id
                && x.StatusId == (int)ProcessStatuses.Processing
                && x.Updated == updated)
            .ToArrayAsync(cToken);

        return result;
    }
    Task<T[]> IPersistenceProcessRepository<IPersistentSql>.GetUnprocessedData<T>(Guid correlationId, IPersistentProcessStep step, int limit, DateTime updateTime, int maxAttempts, Expression<Func<T, bool>> filter, CancellationToken cToken)
    {
        throw new NotImplementedException();
    }
    public async Task SetProcessedData<T>(Guid correlationId, IPersistentProcessStep currentStep, IPersistentProcessStep? nextStep, IEnumerable<T> data, CancellationToken cToken) where T : class, IPersistentSql, IPersistentProcess
    {
        var updated = DateTime.UtcNow;

        var options = new PersistenceUpdateOptions<T>(Update,data)
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
