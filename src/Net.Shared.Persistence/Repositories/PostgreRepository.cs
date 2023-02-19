using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Net.Shared.Persistence.Abstractions.Contexts;
using Net.Shared.Persistence.Abstractions.Entities;
using Net.Shared.Persistence.Abstractions.Entities.Catalogs;
using Net.Shared.Persistence.Abstractions.Repositories;
using Net.Shared.Persistence.Abstractions.Repositories.Parts;

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
        var initiator = $"Postgre repository of '{typeof(TEntity).Name}' by Id {objectId}";

        _reader = new Lazy<IPersistenceReaderRepository<TEntity>>(() => new PostgreReaderRepository<TEntity>(context));
        _writer = new Lazy<IPersistenceWriterRepository<TEntity>>(() => new PostgreWriterRepository<TEntity>(logger, context, initiator));
    }
}
internal sealed class PostgreReaderRepository<TEntity> : IPersistenceReaderRepository<TEntity> where TEntity : IPersistentSql
{
    private readonly IPersistencePostgreContext _context;
    public PostgreReaderRepository(IPersistencePostgreContext context) => _context = context;

    public Task<T?> FindSingleAsync<T>(Expression<Func<T, bool>> condition, CancellationToken cToken = default) where T : class, TEntity =>
        _context.FindSingleAsync(condition, cToken);
    public Task<T?> FindFirstAsync<T>(Expression<Func<T, bool>> condition, CancellationToken cToken = default) where T : class, TEntity =>
        _context.FindFirstAsync(condition, cToken);
    public Task<T[]> FindManyAsync<T>(Expression<Func<T, bool>> condition, CancellationToken cToken = default) where T : class, TEntity =>
        _context.FindManyAsync(condition, cToken);

    public Task<T[]> GetCatalogsAsync<T>(CancellationToken cToken = default) where T : class, IPersistentCatalog, TEntity =>
        _context.FindManyAsync<T>(x => true, cToken);
    public Task<T?> GetCatalogByIdAsync<T>(int id, CancellationToken cToken = default) where T : class, IPersistentCatalog, TEntity =>
        _context.FindSingleAsync<T>(x => x.Id == id, cToken);
    public Task<T?> GetCatalogByNameAsync<T>(string name, CancellationToken cToken = default) where T : class, IPersistentCatalog, TEntity =>
        _context.FindSingleAsync<T>(x => x.Name.Equals(name), cToken);
    public Task<Dictionary<int, T>> GetCatalogsDictionaryByIdAsync<T>(CancellationToken cToken = default) where T : class, IPersistentCatalog, TEntity =>
            Task.Run(() => _context.Set<T>().ToDictionary(x => x.Id));
    public Task<Dictionary<string, T>> GetCatalogsDictionaryByNameAsync<T>(CancellationToken cToken = default) where T : class, IPersistentCatalog, TEntity =>
            Task.Run(() => _context.Set<T>().ToDictionary(x => x.Name));

    public async Task<T[]> GetProcessableAsync<T>(IPersistentProcessStep step, int limit, CancellationToken cToken = default) where T : class, IPersistentProcess, TEntity
    {
        var tableName = _context.Model.FindEntityType(typeof(T))?.ShortName()
           ?? throw new SharedPersistenceException(typeof(T).Name, "Searching a table name", new("Table name not found"));

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

        var ids = await _context.Set<T>().FromSqlRaw(query).Select(x => x.Id).ToArrayAsync(cToken);

        return await _context.Set<T>().Where(x => ids.Contains(x.Id)).ToArrayAsync(cToken);
    }
    public async Task<T[]> GetUnprocessableAsync<T>(IPersistentProcessStep step, int limit, DateTime updateTime, int maxAttempts, CancellationToken cToken = default) where T : class, IPersistentProcess, TEntity
    {
        var tableName = _context.Model.FindEntityType(typeof(T))?.ShortName()
           ?? throw new SharedPersistenceException(typeof(T).Name, "Searching a table name", new("Table name not found"));

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

        var ids = await _context.Set<T>().FromSqlRaw(query).Select(x => x.Id).ToArrayAsync(cToken);

        return await _context.Set<T>().Where(x => ids.Contains(x.Id)).ToArrayAsync(cToken);
    }

}
internal sealed class PostgreWriterRepository<TEntity> : IPersistenceWriterRepository<TEntity> where TEntity : IPersistentSql
{
    private readonly ILogger _logger;
    private readonly IPersistencePostgreContext _context;
    private readonly string _initiator;

    public PostgreWriterRepository(ILogger logger, IPersistencePostgreContext context, string initiator)
    {
        _logger = logger;
        _context = context;
        _initiator = initiator;
    }

    public async Task CreateAsync<T>(T entity, CancellationToken cToken = default) where T : class, TEntity
    {
        await _context.CreateAsync(entity, cToken);

        _logger.LogTrace(_initiator, typeof(T).Name + ' ' + Constants.Actions.Created, Constants.Actions.Success);

    }
    public async Task CreateManyAsync<T>(IReadOnlyCollection<T> entities, CancellationToken cToken = default) where T : class, TEntity
    {
        if (!entities.Any())
        {
            _logger.LogTrace(_initiator, Constants.Actions.Created, Constants.Actions.NoData);
            return;
        }

        await _context.CreateManyAsync(entities, cToken);

        _logger.LogTrace(_initiator, Constants.Actions.Created, Constants.Actions.Success);
    }
    public async Task<TryResult<T>> TryCreateAsync<T>(T entity, CancellationToken cToken = default) where T : class, TEntity
    {
        try
        {
            await CreateAsync(entity, cToken);
            return new TryResult<T>(entity);
        }
        catch (Exception exception)
        {
            return new TryResult<T>(exception);
        }
    }
    public async Task<TryResult<T[]>> TryCreateRangeAsync<T>(IReadOnlyCollection<T> entities, CancellationToken cToken = default) where T : class, TEntity
    {
        try
        {
            await CreateManyAsync(entities, cToken);
            return new TryResult<T[]>(entities.ToArray());
        }
        catch (Exception exception)
        {
            return new TryResult<T[]>(exception);
        }
    }

    public async Task<T[]> UpdateAsync<T>(Expression<Func<T, bool>> condition, T entity, CancellationToken cToken = default) where T : class, TEntity
    {
        var entities = await _context.UpdateAsync(condition, entity, cToken);

        _logger.LogTrace(_initiator, Constants.Actions.Updated, Constants.Actions.Success, entities.Length);

        return entities;
    }
    public async Task<TryResult<T[]>> TryUpdateAsync<T>(Expression<Func<T, bool>> condition, T entity, CancellationToken cToken = default) where T : class, TEntity
    {
        try
        {
            var entities = await UpdateAsync(condition, entity, cToken);

            return new TryResult<T[]>(entities);
        }
        catch (Exception exception)
        {
            return new TryResult<T[]>(exception);
        }
    }

    public async Task<T[]> DeleteAsync<T>(Expression<Func<T, bool>> condition, CancellationToken cToken = default) where T : class, TEntity
    {
        var entities = await _context.DeleteAsync(condition, cToken);

        _logger.LogTrace(_initiator, Constants.Actions.Deleted, Constants.Actions.Success, entities.Length);

        return entities;
    }
    public async Task<TryResult<T[]>> TryDeleteAsync<T>(Expression<Func<T, bool>> condition, CancellationToken cToken = default) where T : class, TEntity
    {
        try
        {
            var entities = await DeleteAsync(condition, cToken);

            return new TryResult<T[]>(entities);
        }
        catch (Exception exception)
        {
            return new TryResult<T[]>(exception);
        }
    }

    public async Task SaveProcessableAsync<T>(IPersistentProcessStep? step, IEnumerable<T> entities, CancellationToken cToken = default) where T : class, IPersistentProcess, TEntity
    {
        try
        {
            await _context.StartTransactionAsync();

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

                await _context.UpdateAsync(x => x.Id == entity.Id, entity, cToken);
                count++;
            }

            await _context.CommitTransactionAsync();

            _logger.LogTrace(_initiator, Constants.Actions.Updated, Constants.Actions.Success, count);
        }
        catch (Exception exception)
        {
            await _context.RollbackTransactionAsync();

            throw new SharedPersistenceException(nameof(PostgreWriterRepository<TEntity>), nameof(SaveProcessableAsync), new(exception));
        }
    }
}
