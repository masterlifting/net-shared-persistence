using System.Linq.Expressions;
using Microsoft.Extensions.Logging;
using Net.Shared.Extensions.Logging;
using Net.Shared.Persistence.Abstractions.Contexts;
using Net.Shared.Persistence.Abstractions.Entities;
using Net.Shared.Persistence.Abstractions.Entities.Catalogs;
using Net.Shared.Persistence.Abstractions.Repositories;
using Net.Shared.Persistence.Contexts;
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
    public Task<T[]> GetProcessSteps<T>(CancellationToken cToken) where T : class, IPersistentNoSql, IPersistentProcessStep =>
        Task.Run(() => _context.SetEntity<T>().ToArray());
    public async Task<T[]> GetProcessableData<T>(IPersistentProcessStep step, int limit, CancellationToken cToken) where T : class, IPersistentNoSql, IPersistentProcess
    {
        Expression<Func<T, bool>> condition = x =>
            x.ProcessStepId == step.Id
            && x.ProcessStatusId == (int)ProcessStatuses.Ready;

        var updater = (T x) =>
        {
            x.Updated = DateTime.UtcNow;
            x.ProcessStatusId = (int)ProcessStatuses.Processing;
            x.ProcessAttempt++;
        };

        return await _context.Update(condition, updater, cToken);
    }
    public async Task<T[]> GetUnprocessedData<T>(IPersistentProcessStep step, int limit, DateTime updateTime, int maxAttempts, CancellationToken cToken) where T : class, IPersistentNoSql, IPersistentProcess
    {
        Expression<Func<T, bool>> condition = x =>
            x.ProcessStepId == step.Id
            && ((x.ProcessStatusId == (int)ProcessStatuses.Processing && x.Updated < updateTime) || x.ProcessStatusId == (int)ProcessStatuses.Error)
            && x.ProcessAttempt < maxAttempts;

        var updater = (T x) =>
        {
            x.Updated = DateTime.UtcNow;
            x.ProcessStatusId = (int)ProcessStatuses.Processing;
            x.ProcessAttempt++;
        };

        return await _context.Update(condition, updater, cToken);
    }
    public async Task SetProcessedData<T>(IPersistentProcessStep? step, IEnumerable<T> entities, CancellationToken cToken) where T : class, IPersistentNoSql, IPersistentProcess
    {
        try
        {
            await _context.StartTransaction(cToken);

            var count = 0;
            foreach (var entity in entities)
            {
                entity.Updated = DateTime.UtcNow;

                if (entity.ProcessStatusId != (int)ProcessStatuses.Error)
                {
                    entity.Error = null;

                    if (step is not null)
                        entity.ProcessStepId = step.Id;
                }

                await _context.Update(x => x.Id == entity.Id, entity, cToken);

                count++;
            }

            await _context.CommitTransaction(cToken);

            _logger.Trace($"The {count} entities were updated.");
        }
        catch (Exception exception)
        {
            await _context.RollbackTransaction(cToken);

            throw new PersistenceException(exception);
        }
    }
    #endregion
}
