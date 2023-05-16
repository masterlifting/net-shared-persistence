using System.Linq.Expressions;

using Microsoft.Extensions.Logging;

using Net.Shared.Persistence.Abstractions.Contexts;
using Net.Shared.Persistence.Abstractions.Entities;
using Net.Shared.Persistence.Abstractions.Entities.Catalogs;
using Net.Shared.Persistence.Abstractions.Repositories;
using Net.Shared.Persistence.Contexts;
using Net.Shared.Persistence.Models.Contexts;

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
    public async Task<T[]> GetProcessableData<T>(Guid hostId, IPersistentProcessStep step, int limit, CancellationToken cToken) where T : class, IPersistentNoSql, IPersistentProcess
    {
        Expression<Func<T, bool>> filter = x =>
            x.HostId == null
            && x.StepId == step.Id
            && x.StatusId == (int)ProcessStatuses.Ready;

        var updated = DateTime.UtcNow;

        var updater = (T x) =>
        {
            x.HostId = hostId;
            x.StatusId = (int)ProcessStatuses.Processing;
            x.Attempt++;
            x.Updated = updated;
        };

        var options = new PersistenceOptions
        {
            Limit = limit
        };

        return await _context.Update(filter, updater, options, cToken);
    }
    public async Task<T[]> GetUnprocessedData<T>(Guid hostId, IPersistentProcessStep step, int limit, DateTime updateTime, int maxAttempts, CancellationToken cToken) where T : class, IPersistentNoSql, IPersistentProcess
    {
        Expression<Func<T, bool>> filter = x =>
            x.HostId == hostId
            && x.StepId == step.Id
            && ((x.StatusId == (int)ProcessStatuses.Processing && x.Updated < updateTime) || x.StatusId == (int)ProcessStatuses.Error)
            && x.Attempt < maxAttempts;

        var updated = DateTime.UtcNow;

        var updater = (T x) =>
        {
            x.StatusId = (int)ProcessStatuses.Processing;
            x.Attempt++;
            x.Updated = updated;
        };

        var options = new PersistenceOptions
        {
            Limit = limit
        };

        return await _context.Update(filter, updater, options, cToken);
    }
    public async Task SetProcessedData<T>(Guid hostId, IPersistentProcessStep? step, IEnumerable<T> entities, CancellationToken cToken) where T : class, IPersistentNoSql, IPersistentProcess
    {
        var entity = entities.First();

        Expression<Func<T, bool>> filter = x =>
            x.HostId == hostId
            && x.StepId == entity.StepId
            && x.Updated == entity.Updated;

        var updated = DateTime.UtcNow;

        foreach (var item in entities)
        {
            item.Updated = updated;

            if (item.StatusId != (int)ProcessStatuses.Error)
            {
                item.Error = null;

                if (step is not null)
                    item.StepId = step.Id;
            }
        }

        await _context.Update(filter, entities, null, cToken);
    }
    #endregion
}
