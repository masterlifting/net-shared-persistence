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

        var result = await _context.SetEntity<T>().Where(x => ids.Contains(x.Id)).ToArrayAsync(cToken);

        _logger.Trace($"The processable data {result} were gotten.");

        return result;
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

        var result = await _context.SetEntity<T>().Where(x => ids.Contains(x.Id)).ToArrayAsync(cToken);

        _logger.Trace($"The unprocessed data {result} were gotten.");

        return result;
    }
    public async Task SetProcessedData<T>(IPersistentProcessStep? step, IEnumerable<T> entities, CancellationToken cToken) where T : class, IPersistentSql, IPersistentProcess
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
    }
    #endregion
}
