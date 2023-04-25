using System.Linq.Expressions;

using Microsoft.Extensions.Logging;
using Net.Shared.Extensions.Logging;
using Net.Shared.Persistence.Abstractions.Contexts;
using Net.Shared.Persistence.Abstractions.Entities;
using Net.Shared.Persistence.Abstractions.Entities.Catalogs;
using Net.Shared.Persistence.Abstractions.Repositories;
using Net.Shared.Persistence.Contexts;

using static Net.Shared.Persistence.Models.Constants.Enums;

namespace Net.Shared.Persistence.Repositories.MongoDb;

public sealed class MongoDbProcessRepository : IPersistenceNoSqlProcessRepository
{
    public MongoDbProcessRepository(ILogger<MongoDbProcessRepository> logger, MongoDbContext context)
    {
        _logger = logger;
        _context = context;
        Context = context;
    }

    #region PRIVATE FIELDS
    private readonly ILogger<MongoDbProcessRepository> _logger;
    private readonly MongoDbContext _context;
    #endregion

    #region PUBLIC PROPERTIES
    public IPersistenceNoSqlContext Context { get; }
    #endregion

    #region PUBLIC METHODS
    public Task<T[]> GetProcessSteps<T>(CancellationToken cToken) where T : class, IPersistentNoSql, IPersistentProcessStep =>
        Task.Run(() => _context.SetEntity<T>().ToArray());
    public async Task<T[]> GetProcessableData<T>(IPersistentProcessStep step, int limit, CancellationToken cToken) where T : class, IPersistentNoSql, IPersistentProcess
    {
        Expression<Func<T, bool>> filter = x =>
            x.ProcessStepId == step.Id
            && x.ProcessStatusId == (int)ProcessStatuses.Ready;

        var updater = (T x) =>
        {
            x.Updated = DateTime.UtcNow;
            x.ProcessStatusId = (int)ProcessStatuses.Processing;
            x.ProcessAttempt++;
        };

        var result = await _context.Update(filter, updater, cToken);

        _logger.Trace($"The processable data {result} were gotten.");

        return result;
    }
    public async Task<T[]> GetUnprocessedData<T>(IPersistentProcessStep step, int limit, DateTime updateTime, int maxAttempts, CancellationToken cToken) where T : class, IPersistentNoSql, IPersistentProcess
    {
        Expression<Func<T, bool>> filter = x =>
            x.ProcessStepId == step.Id
            && ((x.ProcessStatusId == (int)ProcessStatuses.Processing && x.Updated < updateTime) || x.ProcessStatusId == (int)ProcessStatuses.Error)
            && x.ProcessAttempt < maxAttempts;

        var updater = (T x) =>
        {
            x.Updated = DateTime.UtcNow;
            x.ProcessStatusId = (int)ProcessStatuses.Processing;
            x.ProcessAttempt++;
        };

        var result = await _context.Update(filter, updater, cToken);

        _logger.Trace($"The unprocessed data {result} were gotten.");

        return result;
    }
    public async Task SetProcessedData<T>(IPersistentProcessStep? step, IEnumerable<T> entities, CancellationToken cToken) where T : class, IPersistentNoSql, IPersistentProcess
    {
        Expression<Func<T, bool>> filter = x => entities.Select(y => y.Id).Contains(x.Id);

        var dateUpdate = DateTime.UtcNow;

        var updater = (T x) =>
        {
            x.Updated = dateUpdate;

            if (x.ProcessStatusId != (int)ProcessStatuses.Error)
            {
                x.Error = null;

                if (step is not null)
                    x.ProcessStepId = step.Id;
            }
        };

        var result = await _context.Update(filter, updater, cToken);

        _logger.Trace($"The processed data {result} were updated.");

        return;
    }
    #endregion
}
