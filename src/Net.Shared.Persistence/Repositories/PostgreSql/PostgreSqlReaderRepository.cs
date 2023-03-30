using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Net.Shared.Persistence.Abstractions.Entities;
using Net.Shared.Persistence.Abstractions.Entities.Catalogs;
using Net.Shared.Persistence.Abstractions.Repositories.Sql;
using Net.Shared.Persistence.Contexts;
using static Net.Shared.Persistence.Models.Constants.Enums;

namespace Net.Shared.Persistence.Repositories.PostgreSql;

public sealed class PostgreSqlReaderRepository<TEntity> : IPersistenceSqlReaderRepository<TEntity> where TEntity : class, IPersistentSql
{
    private readonly PostgreSqlContext _context;
    public PostgreSqlReaderRepository(PostgreSqlContext context) => _context = context;

    public Task<T?> FindSingle<T>(Expression<Func<T, bool>> filter, CancellationToken cToken) where T : class, TEntity =>
        _context.FindSingle(filter, cToken);
    public Task<T?> FindFirst<T>(Expression<Func<T, bool>> filter, CancellationToken cToken) where T : class, TEntity =>
        _context.FindFirst(filter, cToken);
    public Task<T[]> FindMany<T>(Expression<Func<T, bool>> filter, CancellationToken cToken) where T : class, TEntity =>
        _context.FindMany(filter, cToken);

    public Task<T[]> GetCatalogs<T>(CancellationToken cToken) where T : class, IPersistentCatalog, TEntity =>
        _context.FindMany<T>(x => true, cToken);
    public Task<T?> GetCatalogById<T>(int id, CancellationToken cToken) where T : class, IPersistentCatalog, TEntity =>
        _context.FindSingle<T>(x => x.Id == id, cToken);
    public Task<T?> GetCatalogByName<T>(string name, CancellationToken cToken) where T : class, IPersistentCatalog, TEntity =>
        _context.FindSingle<T>(x => x.Name.Equals(name), cToken);
    public Task<Dictionary<int, T>> GetCatalogsDictionaryById<T>(CancellationToken cToken) where T : class, IPersistentCatalog, TEntity =>
            Task.Run(() => _context.SetEntity<T>().ToDictionary(x => x.Id));
    public Task<Dictionary<string, T>> GetCatalogsDictionaryByName<T>(CancellationToken cToken) where T : class, IPersistentCatalog, TEntity =>
            Task.Run(() => _context.SetEntity<T>().ToDictionary(x => x.Name));

    public async Task<T[]> GetProcessableData<T>(IPersistentProcessStep step, int limit, CancellationToken cToken) where T : class, IPersistentProcess, TEntity
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
    public async Task<T[]> GetUnprocessableData<T>(IPersistentProcessStep step, int limit, DateTime updateTime, int maxAttempts, CancellationToken cToken) where T : class, IPersistentProcess, TEntity
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
}
