using Microsoft.EntityFrameworkCore;

using Net.Shared.Persistence.Abstractions.Interfaces.Contexts;
using Net.Shared.Persistence.Abstractions.Interfaces.Entities;
using Net.Shared.Persistence.Abstractions.Interfaces.Entities.Catalogs;
using Net.Shared.Persistence.Abstractions.Interfaces.Repositories.Sql;
using Net.Shared.Persistence.Abstractions.Models.Contexts;
using Net.Shared.Persistence.Contexts;

using static Net.Shared.Persistence.Abstractions.Constants.Enums;

namespace Net.Shared.Persistence.Repositories.PostgreSql;

public sealed class PostgreSqlProcessRepository : IPersistenceSqlProcessRepository
{
    public PostgreSqlProcessRepository(PostgreSqlContext context)
    {
        _context = context;
        Context = context;
    }

    #region PRIVATE FIELDS
    private readonly PostgreSqlContext _context;
    #endregion

    #region PUBLIC PROPERTIES
    public IPersistenceSqlContext Context { get; }
    #endregion

    #region PUBLIC METHODS
    public Task<T[]> GetProcessSteps<T>(CancellationToken cToken) where T : class, IPersistentSql, IPersistentProcessStep =>
        _context.FindMany<T>(new(), cToken);
    public async Task<T[]> GetProcessableData<T>(Guid hostId, IPersistentProcessStep step, int limit, CancellationToken cToken) where T : class, IPersistentSql, IPersistentProcess
    {
        var updated = DateTime.UtcNow;

        var updatedCount = await _context.GetQuery<T>()
            .Where(x =>
                x.HostId == null || x.HostId == hostId
                && x.StepId == step.Id
                && x.StatusId == (int)ProcessStatuses.Ready)
            .Take(limit)
            .ExecuteUpdateAsync(x => x
                .SetProperty(y => y.HostId, hostId)
                .SetProperty(y => y.StatusId, (int)ProcessStatuses.Processing)
                .SetProperty(y => y.Attempt, y => y.Attempt + 1)
                .SetProperty(y => y.Updated, updated)
            , cToken);

        var result = await _context.GetQuery<T>()
            .Where(x =>
                x.HostId == hostId
                && x.StepId == step.Id
                && x.StatusId == (int)ProcessStatuses.Processing
                && x.Updated == updated)
            .ToArrayAsync(cToken);

        return result;
    }
    public async Task<T[]> GetUnprocessedData<T>(Guid hostId, IPersistentProcessStep step, int limit, DateTime updateTime, int maxAttempts, CancellationToken cToken) where T : class, IPersistentSql, IPersistentProcess
    {
        var updated = DateTime.UtcNow;

        var updatedCount = await _context.GetQuery<T>()
            .Where(x =>
                x.HostId == hostId
                && x.StepId == step.Id
                && ((x.StatusId == (int)ProcessStatuses.Processing && x.Updated < updateTime) || x.StatusId == (int)ProcessStatuses.Error)
                && x.Attempt < maxAttempts)
            .Take(limit)
            .ExecuteUpdateAsync(x => x
                .SetProperty(y => y.StatusId, (int)ProcessStatuses.Processing)
                .SetProperty(y => y.Attempt, y => y.Attempt + 1)
                .SetProperty(y => y.Updated, updated)
            , cToken);

        var result = await _context.GetQuery<T>()
            .Where(x =>
                x.HostId == hostId
                && x.StepId == step.Id
                && x.StatusId == (int)ProcessStatuses.Processing
                && x.Updated == updated)
            .ToArrayAsync(cToken);

        return result;
    }
    public async Task SetProcessedData<T>(Guid hostId, IPersistentProcessStep currentStep, IPersistentProcessStep? nextStep, IEnumerable<T> data, CancellationToken cToken) where T : class, IPersistentSql, IPersistentProcess
    {
        var updated = DateTime.UtcNow;

        var updater = (T x) =>
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
        };

        var options = new PersistenceUpdateOptions<T>(updater,data)
        {
            QueryOptions = new()
            {
                Filter = x =>
                    x.HostId == hostId
                    && x.StepId == currentStep.Id
                    && x.StatusId == (int)ProcessStatuses.Processing
            }
        };

        await _context.Update(options, cToken);
    }
    #endregion
}
