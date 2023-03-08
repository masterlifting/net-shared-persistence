using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Net.Shared.Models.Domain;
using Net.Shared.Persistence.Abstractions.Core.Contexts;
using Net.Shared.Persistence.Abstractions.Core.Repositories;
using Net.Shared.Persistence.Abstractions.Core.Repositories.Parts;
using Net.Shared.Persistence.Abstractions.Entities;
using Net.Shared.Persistence.Abstractions.Entities.Catalogs;
using Net.Shared.Persistence.Models.Exceptions;

using System.Linq.Expressions;

using static Net.Shared.Persistence.Models.Constants.Enums;

namespace Net.Shared.Persistence.Repositories;

public abstract class PostgreRepository<TEntity> : IPersistenceSqlRepository<TEntity>
    where TEntity : class, IPersistentSql
{
    private readonly Lazy<IPersistenceReaderRepository<TEntity>> _reader;
    private readonly Lazy<IPersistenceWriterRepository<TEntity>> _writer;

    public IPersistenceReaderRepository<TEntity> Reader { get => _reader.Value; }
    public IPersistenceWriterRepository<TEntity> Writer { get => _writer.Value; }

    protected PostgreRepository(ILogger<TEntity> logger, IPersistencePostgreContext context)
    {
        var objectId = base.GetHashCode();
        var repositoryInfo = $"Postgre repository {objectId} of '{typeof(TEntity).Name}'";

        _reader = new Lazy<IPersistenceReaderRepository<TEntity>>(() => new PostgreReaderRepository<TEntity>(context));
        _writer = new Lazy<IPersistenceWriterRepository<TEntity>>(() => new PostgreWriterRepository<TEntity>(logger, context, repositoryInfo));
    }
}
internal sealed class PostgreReaderRepository<TEntity> : IPersistenceReaderRepository<TEntity> where TEntity : class, IPersistentSql
{
    private readonly IPersistencePostgreContext _context;
    public PostgreReaderRepository(IPersistencePostgreContext context) => _context = context;

    public Task<T?> FindSingle<T>(Expression<Func<T, bool>> condition, CancellationToken cToken = default) where T : class, TEntity =>
        _context.FindSingle(condition, cToken);
    public Task<T?> FindFirst<T>(Expression<Func<T, bool>> condition, CancellationToken cToken = default) where T : class, TEntity =>
        _context.FindFirst(condition, cToken);
    public Task<T[]> FindMany<T>(Expression<Func<T, bool>> condition, CancellationToken cToken = default) where T : class, TEntity =>
        _context.FindMany(condition, cToken);

    public Task<T[]> GetCatalogs<T>(CancellationToken cToken = default) where T : class, IPersistentCatalog, TEntity =>
        _context.FindMany<T>(x => true, cToken);
    public Task<T?> GetCatalogById<T>(int id, CancellationToken cToken = default) where T : class, IPersistentCatalog, TEntity =>
        _context.FindSingle<T>(x => x.Id == id, cToken);
    public Task<T?> GetCatalogByName<T>(string name, CancellationToken cToken = default) where T : class, IPersistentCatalog, TEntity =>
        _context.FindSingle<T>(x => x.Name.Equals(name), cToken);
    public Task<Dictionary<int, T>> GetCatalogsDictionaryById<T>(CancellationToken cToken = default) where T : class, IPersistentCatalog, TEntity =>
            Task.Run(() => _context.Set<T>().ToDictionary(x => x.Id));
    public Task<Dictionary<string, T>> GetCatalogsDictionaryByName<T>(CancellationToken cToken = default) where T : class, IPersistentCatalog, TEntity =>
            Task.Run(() => _context.Set<T>().ToDictionary(x => x.Name));

    public async Task<T[]> GetProcessableData<T>(IPersistentProcessStep step, int limit, CancellationToken cToken = default) where T : class, IPersistentProcess, TEntity
    {
        var tableName = _context.GetTableName<T>();

        var query = @$"
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

        var ids = await _context.FromSqlRaw<T>(query).Select(x => x.Id).ToArrayAsync(cToken);

        return await _context.Set<T>().Where(x => ids.Contains(x.Id)).ToArrayAsync(cToken);
    }
    public async Task<T[]> GetUnprocessableData<T>(IPersistentProcessStep step, int limit, DateTime updateTime, int maxAttempts, CancellationToken cToken = default) where T : class, IPersistentProcess, TEntity
    {
        var tableName = _context.GetTableName<T>();

        var query = @$"
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

        var ids = await _context.FromSqlRaw<T>(query).Select(x => x.Id).ToArrayAsync(cToken);

        return await _context.Set<T>().Where(x => ids.Contains(x.Id)).ToArrayAsync(cToken);
    }

}
internal sealed class PostgreWriterRepository<TEntity> : IPersistenceWriterRepository<TEntity> where TEntity : class, IPersistentSql
{
    private readonly ILogger _logger;
    private readonly IPersistencePostgreContext _context;
    private readonly string _repositoryInfo;

    public PostgreWriterRepository(ILogger logger, IPersistencePostgreContext context, string repositoryInfo)
    {
        _logger = logger;
        _context = context;
        _repositoryInfo = repositoryInfo;
    }

    public async Task CreateOne<T>(T entity, CancellationToken cToken = default) where T : class, TEntity
    {
        await _context.CreateOne(entity, cToken);

        _logger.LogTrace(_repositoryInfo, typeof(T).Name + ' ' + Constants.Actions.Created, Constants.Actions.Success);

    }
    public async Task CreateMany<T>(IReadOnlyCollection<T> entities, CancellationToken cToken = default) where T : class, TEntity
    {
        if (!entities.Any())
        {
            _logger.LogTrace(_repositoryInfo, Constants.Actions.Created, Constants.Actions.NoData);
            return;
        }

        await _context.CreateMany(entities, cToken);

        _logger.LogTrace(_repositoryInfo, Constants.Actions.Created, Constants.Actions.Success);
    }
    public async Task<TryResult<T>> TryCreateOne<T>(T entity, CancellationToken cToken = default) where T : class, TEntity
    {
        try
        {
            await CreateOne(entity, cToken);
            return new TryResult<T>(entity);
        }
        catch (Exception exception)
        {
            return new TryResult<T>(exception);
        }
    }
    public async Task<TryResult<T[]>> TryCreateMany<T>(IReadOnlyCollection<T> entities, CancellationToken cToken = default) where T : class, TEntity
    {
        try
        {
            await CreateMany(entities, cToken);
            return new TryResult<T[]>(entities.ToArray());
        }
        catch (Exception exception)
        {
            return new TryResult<T[]>(exception);
        }
    }

    public async Task<T[]> Update<T>(Expression<Func<T, bool>> condition, T entity, CancellationToken cToken = default) where T : class, TEntity
    {
        var entities = await _context.Update(condition, entity, cToken);

        _logger.LogTrace(_repositoryInfo, Constants.Actions.Updated, Constants.Actions.Success, entities.Length);

        return entities;
    }
    public async Task<TryResult<T[]>> TryUpdate<T>(Expression<Func<T, bool>> condition, T entity, CancellationToken cToken = default) where T : class, TEntity
    {
        try
        {
            var entities = await Update(condition, entity, cToken);

            return new TryResult<T[]>(entities);
        }
        catch (Exception exception)
        {
            return new TryResult<T[]>(exception);
        }
    }

    public async Task<T[]> Delete<T>(Expression<Func<T, bool>> condition, CancellationToken cToken = default) where T : class, TEntity
    {
        var entities = await _context.Delete(condition, cToken);

        _logger.LogTrace(_repositoryInfo, Constants.Actions.Deleted, Constants.Actions.Success, entities.Length);

        return entities;
    }
    public async Task<TryResult<T[]>> TryDelete<T>(Expression<Func<T, bool>> condition, CancellationToken cToken = default) where T : class, TEntity
    {
        try
        {
            var entities = await Delete(condition, cToken);

            return new TryResult<T[]>(entities);
        }
        catch (Exception exception)
        {
            return new TryResult<T[]>(exception);
        }
    }

    public async Task SaveProcessableData<T>(IPersistentProcessStep? step, IEnumerable<T> entities, CancellationToken cToken = default) where T : class, IPersistentProcess, TEntity
    {
        try
        {
            await _context.StartTransaction();

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

            await _context.CommitTransaction();

            _logger.LogTrace(_repositoryInfo, Constants.Actions.Updated, Constants.Actions.Success, count);
        }
        catch (Exception exception)
        {
            await _context.RollbackTransaction();

            throw new NetSharedPersistenceException(exception);
        }
    }
}
