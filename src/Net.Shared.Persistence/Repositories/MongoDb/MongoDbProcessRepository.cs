using Microsoft.Extensions.Logging;
using Net.Shared.Extensions.Logging;
using Net.Shared.Persistence.Abstractions.Contexts;
using Net.Shared.Persistence.Abstractions.Entities;
using Net.Shared.Persistence.Abstractions.Entities.Catalogs;
using Net.Shared.Persistence.Abstractions.Repositories;
using Net.Shared.Persistence.Contexts;
using Net.Shared.Persistence.Models.Contexts;
using Net.Shared.Persistence.Models.Exceptions;
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
    public Task<T[]> GetProcessSteps<T>(CancellationToken cToken = default) where T : class, IPersistentNoSql, IPersistentProcessStep =>
        _context.FindMany<T>(new(), cToken);
    public async Task<T[]> GetProcessableData<T>(Guid hostId, IPersistentProcessStep step, int limit, CancellationToken cToken = default) where T : class, IPersistentNoSql, IPersistentProcess
    {
        var updated = DateTime.UtcNow;

        var updater = (T x) =>
        {
            x.HostId = hostId;
            x.StatusId = (int)ProcessStatuses.Processing;
            x.Attempt++;
            x.Updated = updated;
        };

        var options = new PersistenceQueryOptions<T>
        {
            Filter = x =>
                x.HostId == null
                && x.StepId == step.Id
                && x.StatusId == (int)ProcessStatuses.Ready,
            Take = limit
        };

        return await _context.Update(options, updater, cToken);
    }
    public async Task<T[]> GetUnprocessedData<T>(Guid hostId, IPersistentProcessStep step, int limit, DateTime updateTime, int maxAttempts, CancellationToken cToken = default) where T : class, IPersistentNoSql, IPersistentProcess
    {
        var updated = DateTime.UtcNow;

        var updater = (T x) =>
        {
            x.StatusId = (int)ProcessStatuses.Processing;
            x.Attempt++;
            x.Updated = updated;
        };

        var options = new PersistenceQueryOptions<T>
        {
            Filter = x =>
                x.HostId == hostId
                && x.StepId == step.Id
                && ((x.StatusId == (int)ProcessStatuses.Processing && x.Updated < updateTime) || x.StatusId == (int)ProcessStatuses.Error)
                && x.Attempt < maxAttempts,
            Take = limit
        };

        return await _context.Update(options, updater, cToken);
    }
    public async Task SetProcessedData<T>(Guid hostId, IPersistentProcessStep currenttStep, IPersistentProcessStep? nextStep, IEnumerable<T> data, CancellationToken cToken = default) where T : class, IPersistentNoSql, IPersistentProcess
    {
        var updated = DateTime.UtcNow;

        if (nextStep is not null)
        {
            foreach (var item in data)
            {
                if (item.StatusId != (int)ProcessStatuses.Error)
                {
                    item.StepId = nextStep.Id;
                    item.StatusId = (int)ProcessStatuses.Ready;
                    item.Error = null;
                }
                else
                {
                    _logger.Error(new PersistenceException($"Process by step '{currenttStep.Name}' has the error: {item.Error}"));
                }

                item.Updated = updated;
            }
        }
        else
        {
            foreach (var item in data)
            {
                if (item.StatusId != (int)ProcessStatuses.Error)
                {
                    item.StatusId = (int)ProcessStatuses.Completed;
                    item.Error = null;
                }
                else
                {
                    _logger.Error(new PersistenceException($"Process by step '{currenttStep.Name}' has the error: {item.Error}"));
                }

                item.Updated = updated;
            }
        }

        var entity = data.First();

        var options = new PersistenceQueryOptions<T>
        {
            Filter = x =>
                x.HostId == hostId
                && x.StepId == currenttStep.Id
                && x.StatusId == (int)ProcessStatuses.Processing
        };

        await _context.Update(options, data, cToken);
    }
    #endregion
}
