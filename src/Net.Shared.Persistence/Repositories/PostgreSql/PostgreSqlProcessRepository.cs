using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Net.Shared.Extensions.Logging;
using Net.Shared.Persistence.Abstractions.Contexts;
using Net.Shared.Persistence.Abstractions.Entities;
using Net.Shared.Persistence.Abstractions.Entities.Catalogs;
using Net.Shared.Persistence.Abstractions.Repositories;
using Net.Shared.Persistence.Contexts;
using static Net.Shared.Persistence.Models.Constants.Enums;

namespace Net.Shared.Persistence.Repositories.PostgreSql;

public sealed class PostgreSqlProcessRepository : IPersistenceSqlProcessRepository
{
    public PostgreSqlProcessRepository(ILogger<PostgreSqlProcessRepository> logger, PostgreSqlContext context)
    {
        _logger = logger;
        _context = context;
        Context = context;
    }

    #region PRIVATE FIELDS
    private readonly PostgreSqlContext _context;
    private readonly ILogger _logger;
    #endregion

    #region PUBLIC PROPERTIES
    public IPersistenceSqlContext Context { get; }
    #endregion

    #region PUBLIC METHODS
    public Task<T[]> GetProcessSteps<T>(CancellationToken cToken) where T : class, IPersistentSql, IPersistentProcessStep =>
        _context.SetEntity<T>().ToArrayAsync(cToken);
    public async Task<T[]> GetProcessableData<T>(Guid hostId, IPersistentProcessStep step, int limit, CancellationToken cToken) where T : class, IPersistentSql, IPersistentProcess
    {
        var updated = DateTime.UtcNow;

        var updatedCount = await _context.SetEntity<T>()
            .Where(x =>
                x.ProcessHostId == hostId
                && x.ProcessStepId == step.Id
                && x.ProcessStatusId == (int)ProcessStatuses.Ready)
            .Take(limit)
            .ExecuteUpdateAsync(x => x
                .SetProperty(y => y.ProcessStatusId, (int)ProcessStatuses.Processing)
                .SetProperty(y => y.ProcessAttempt, y => y.ProcessAttempt + 1)
                .SetProperty(y => y.Updated, updated)
            , cToken);

        var result = await _context.SetEntity<T>()
            .Where(x =>
                x.ProcessHostId == hostId
                && x.ProcessStepId == step.Id
                && x.ProcessStatusId == (int)ProcessStatuses.Processing
                && x.Updated == updated)
            .ToArrayAsync(cToken);

        _logger.Trace($"The processable data were updated and received. Items count - {result.Length}.");

        if (result.Length != updatedCount)
            _logger.Warning($"The processable data were updated. Items count - {updatedCount}, but received - {result.Length}.");

        return result;
    }
    public async Task<T[]> GetUnprocessedData<T>(Guid hostId, IPersistentProcessStep step, int limit, DateTime updateTime, int maxAttempts, CancellationToken cToken) where T : class, IPersistentSql, IPersistentProcess
    {
        var updated = DateTime.UtcNow;

        var updatedCount = await _context.SetEntity<T>()
            .Where(x =>
                x.ProcessHostId == hostId
                && x.ProcessStepId == step.Id
                && ((x.ProcessStatusId == (int)ProcessStatuses.Processing && x.Updated < updateTime) || x.ProcessStatusId == (int)ProcessStatuses.Error)
                && x.ProcessAttempt < maxAttempts)
            .Take(limit)
            .ExecuteUpdateAsync(x => x
                .SetProperty(y => y.ProcessStatusId, (int)ProcessStatuses.Processing)
                .SetProperty(y => y.ProcessAttempt, y => y.ProcessAttempt + 1)
                .SetProperty(y => y.Updated, updated)
            , cToken);

        var result = await _context.SetEntity<T>()
            .Where(x =>
                x.ProcessHostId == hostId
                && x.ProcessStepId == step.Id
                && x.ProcessStatusId == (int)ProcessStatuses.Processing
                && x.Updated == updated)
            .ToArrayAsync(cToken);

        _logger.Trace($"The unprocessed data were updated and received. Items count - {result.Length}.");

        if (result.Length != updatedCount)
            _logger.Warning($"The unprocessed data were updated. Items count - {updatedCount}, but received - {result.Length}.");

        return result;
    }
    public async Task SetProcessedData<T>(Guid hostId, IPersistentProcessStep? step, IEnumerable<T> entities, CancellationToken cToken) where T : class, IPersistentSql, IPersistentProcess
    {
        var entity = entities.First();

        Expression<Func<T, bool>> filter = x =>
            x.ProcessHostId == hostId
            && x.ProcessStepId == entity.ProcessStepId
            && x.ProcessAttempt == entity.ProcessAttempt
            && x.Updated == entity.Updated;

        var updated = DateTime.UtcNow;

        var updater = (T x) =>
        {
            x.Updated = updated;

            if (x.ProcessStatusId != (int)ProcessStatuses.Error)
            {
                x.Error = null;

                if (step is not null)
                    x.ProcessStepId = step.Id;
            }
        };

        var result = await _context.Update(filter, updater, cToken);

        _logger.Trace($"The processed data {result} were updated.");
    }
    #endregion
}
