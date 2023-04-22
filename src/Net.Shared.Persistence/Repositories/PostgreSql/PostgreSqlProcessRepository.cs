using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Net.Shared.Extensions.Logging;
using Net.Shared.Persistence.Abstractions.Contexts;
using Net.Shared.Persistence.Abstractions.Entities;
using Net.Shared.Persistence.Abstractions.Entities.Catalogs;
using Net.Shared.Persistence.Abstractions.Repositories;
using Net.Shared.Persistence.Contexts;
using Net.Shared.Persistence.Models.Exceptions;
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
    public async Task<T[]> GetProcessableData<T>(IPersistentProcessStep step, int limit, CancellationToken cToken) where T : class, IPersistentSql, IPersistentProcess
    {
        var tableName = _context.GetTableName<T>();

        FormattableString query = @$"
                UPDATE ""{tableName}"" SET
                      ""{nameof(IPersistentProcess.ProcessStatusId)}"" = {(int)ProcessStatuses.Processing}
                    , ""{nameof(IPersistentProcess.ProcessAttempt)}"" = ""{nameof(IPersistentProcess.ProcessAttempt)}"" + 1
                    , ""{nameof(IPersistentProcess.Updated)}"" = NOW()
                WHERE ""{nameof(IPersistentProcess.Id)}"" IN 
                    ( SELECT ""{nameof(IPersistentProcess.Id)}""
                      FROM ""{tableName}""
                      WHERE ""{nameof(IPersistentProcess.ProcessStepId)}"" = {step.Id} AND ""{nameof(IPersistentProcess.ProcessStatusId)}"" = {(int)ProcessStatuses.Ready} 
                      LIMIT {limit}
                      FOR UPDATE SKIP LOCKED )
                RETURNING ""{nameof(IPersistentProcess.Id)}"";";

        var ids = await _context.GetQueryFromRaw<T>(query, cToken).Select(x => x.Id).ToArrayAsync(cToken);

        return await _context.SetEntity<T>().Where(x => ids.Contains(x.Id)).ToArrayAsync(cToken);
    }
    public async Task<T[]> GetUnprocessedData<T>(IPersistentProcessStep step, int limit, DateTime updateTime, int maxAttempts, CancellationToken cToken) where T : class, IPersistentSql, IPersistentProcess
    {
        var tableName = _context.GetTableName<T>();

        FormattableString query = @$"
                UPDATE ""{tableName}"" SET
                      ""{nameof(IPersistentProcess.ProcessStatusId)}"" = {(int)ProcessStatuses.Processing}
                    , ""{nameof(IPersistentProcess.ProcessAttempt)}"" = ""{nameof(IPersistentProcess.ProcessAttempt)}"" + 1
                    , ""{nameof(IPersistentProcess.Updated)}"" = NOW()
                WHERE ""{nameof(IPersistentProcess.Id)}"" IN 
                    ( SELECT ""{nameof(IPersistentProcess.Id)}""
                      FROM ""{tableName}""
                      WHERE 
                            ""{nameof(IPersistentProcess.ProcessStepId)}"" = {step.Id} 
                            AND ((""{nameof(IPersistentProcess.ProcessStatusId)}"" = {(int)ProcessStatuses.Processing} AND ""{nameof(IPersistentProcess.Updated)}"" < '{updateTime: yyyy-MM-dd HH:mm:ss}') OR (""{nameof(IPersistentProcess.ProcessStatusId)}"" = {(int)ProcessStatuses.Error}))
    		                AND ""{nameof(IPersistentProcess.ProcessAttempt)}"" < {maxAttempts}
                      LIMIT {limit}
                      FOR UPDATE SKIP LOCKED )
                RETURNING ""{nameof(IPersistentProcess.Id)}"";";

        var ids = await _context.GetQueryFromRaw<T>(query, cToken).Select(x => x.Id).ToArrayAsync(cToken);

        return await _context.SetEntity<T>().Where(x => ids.Contains(x.Id)).ToArrayAsync(cToken);
    }
    public async Task SetProcessedData<T>(IPersistentProcessStep? step, IEnumerable<T> entities, CancellationToken cToken) where T : class, IPersistentSql, IPersistentProcess
    {
        try
        {
            await _context.StartTransaction(cToken);

            var count = 0;
            var dateUpdated = DateTime.UtcNow;

            foreach (var entity in entities)
            {
                entity.Updated = dateUpdated;

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

            _logger.Trace($"Updated {count} entities in {typeof(T).Name} table");
        }
        catch (Exception exception)
        {
            await _context.RollbackTransaction(cToken);

            throw new PersistenceException(exception);
        }
    }
    #endregion
}
