using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Net.Shared.Persistence.Abstractions.Entities;
using Net.Shared.Persistence.Abstractions.Entities.Catalogs;
using Net.Shared.Persistence.Abstractions.Repositories;
using Net.Shared.Persistence.Contexts;
using Net.Shared.Persistence.Models.Exceptions;
using static Net.Shared.Persistence.Models.Constants.Enums;

namespace Net.Shared.Persistence.Repositories.PostgreSql;

public sealed class PostgreSqlProcessRepository : IPersistenceProcessRepository
{
    private readonly PostgreSqlContext _context;
    private readonly ILogger _logger;

    public PostgreSqlProcessRepository(PostgreSqlContext context, ILogger<PostgreSqlProcessRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    Task<T[]> IPersistenceProcessRepository.GetProcessableData<T>(IPersistentProcessStep step, int limit, CancellationToken cToken)
    {
        throw new NotImplementedException();
    }

    Task<T[]> IPersistenceProcessRepository.GetSteps<T>(CancellationToken cToken)
    {
        throw new NotImplementedException();
    }

    Task<T[]> IPersistenceProcessRepository.GetUnprocessableData<T>(IPersistentProcessStep step, int limit, DateTime updateTime, int maxAttempts, CancellationToken cToken)
    {
        throw new NotImplementedException();
    }

    Task IPersistenceProcessRepository.SetProcessableData<T>(IPersistentProcessStep? step, IEnumerable<T> entities, CancellationToken cToken)
    {
        throw new NotImplementedException();
    }

    // Task<Dictionary<string, T>> IPersistenceProcessRepository.GetSteps<T>(CancellationToken cToken) =>
    //     _context.SetEntity<T>().ToDictionaryAsync(x => x.Name, cToken);
    // async Task<T[]> IPersistenceProcessRepository.GetProcessableData<T>(IPersistentProcessStep step, int limit, CancellationToken cToken)
    // {
    //     var tableName = _context.GetTableName<T>();

    //     FormattableString query = @$"
    //             UPDATE ""{tableName}"" SET
    //                   ""{nameof(IPersistentProcess.ProcessStatusId)}"" = {(int)ProcessStatuses.Processing}
    //                 , ""{nameof(IPersistentProcess.ProcessAttempt)}"" = ""{nameof(IPersistentProcess.ProcessAttempt)}"" + 1
    //                 , ""{nameof(IPersistentProcess.Updated)}"" = NOW()
    //             WHERE ""{nameof(IPersistentProcess.Id)}"" IN 
    //                 ( SELECT ""{nameof(IPersistentProcess.Id)}""
    //                   FROM ""{tableName}""
    //                   WHERE ""{nameof(IPersistentProcess.ProcessStepId)}"" = {step.Id} AND ""{nameof(IPersistentProcess.ProcessStatusId)}"" = {(int)ProcessStatuses.Ready} 
    //                   LIMIT {limit}
    //                   FOR UPDATE SKIP LOCKED )
    //             RETURNING ""{nameof(IPersistentProcess.Id)}"";";

    //     var ids = await _context.GetQueryFromRaw<T>(query, cToken).Select(x => x.Id).ToArrayAsync(cToken);

    //     return await _context.SetEntity<T>().Where(x => ids.Contains(x.Id)).ToArrayAsync(cToken);
    // }
    // async Task<T[]> IPersistenceProcessRepository.GetUnprocessableData<T>(IPersistentProcessStep step, int limit, DateTime updateTime, int maxAttempts, CancellationToken cToken)
    // {
    //     var tableName = _context.GetTableName<T>();

    //     FormattableString query = @$"
    //             UPDATE ""{tableName}"" SET
    //                   ""{nameof(IPersistentProcess.ProcessStatusId)}"" = {(int)ProcessStatuses.Processing}
    //                 , ""{nameof(IPersistentProcess.ProcessAttempt)}"" = ""{nameof(IPersistentProcess.ProcessAttempt)}"" + 1
    //                 , ""{nameof(IPersistentProcess.Updated)}"" = NOW()
    //             WHERE ""{nameof(IPersistentProcess.Id)}"" IN 
    //                 ( SELECT ""{nameof(IPersistentProcess.Id)}""
    //                   FROM ""{tableName}""
    //                   WHERE 
    //                         ""{nameof(IPersistentProcess.ProcessStepId)}"" = {step.Id} 
    //                         AND ((""{nameof(IPersistentProcess.ProcessStatusId)}"" = {(int)ProcessStatuses.Processing} AND ""{nameof(IPersistentProcess.Updated)}"" < '{updateTime: yyyy-MM-dd HH:mm:ss}') OR (""{nameof(IPersistentProcess.ProcessStatusId)}"" = {(int)ProcessStatuses.Error}))
    // 		                AND ""{nameof(IPersistentProcess.ProcessAttempt)}"" < {maxAttempts}
    //                   LIMIT {limit}
    //                   FOR UPDATE SKIP LOCKED )
    //             RETURNING ""{nameof(IPersistentProcess.Id)}"";";

    //     var ids = await _context.GetQueryFromRaw<T>(query, cToken).Select(x => x.Id).ToArrayAsync(cToken);

    //     return await _context.SetEntity<T>().Where(x => ids.Contains(x.Id)).ToArrayAsync(cToken);
    // }
    // async Task IPersistenceProcessRepository.SetProcessableData<T>(IPersistentProcessStep? step, IEnumerable<T> entities, CancellationToken cToken)
    // {
    //     try
    //     {
    //         await _context.StartTransaction();

    //         var count = 0;
    //         foreach (var entity in entities)
    //         {
    //             entity.Updated = DateTime.UtcNow;

    //             if (entity.ProcessStatusId != (int)ProcessStatuses.Error)
    //             {
    //                 entity.Error = null;

    //                 if (step is not null)
    //                     entity.ProcessStepId = step.Id;
    //             }

    //             await _context.Update(x => x.Id == entity.Id, entity, cToken);
    //             count++;
    //         }

    //         await _context.CommitTransaction();

    //         _logger.LogTrace($"Updated {count} entities in {typeof(T).Name} table");
    //     }
    //     catch (Exception exception)
    //     {
    //         await _context.RollbackTransaction();

    //         throw new NetSharedPersistenceException(exception);
    //     }
    // }
}
