using Microsoft.Extensions.Logging;

using MongoDB.Driver;

using Shared.Extensions.Logging;
using Shared.Models.Results;
using Shared.Persistence.Abstractions.Contexts;
using Shared.Persistence.Abstractions.Entities;
using Shared.Persistence.Abstractions.Entities.Catalogs;
using Shared.Persistence.Abstractions.Repositories;
using Shared.Persistence.Abstractions.Repositories.Parts;
using Shared.Persistence.Exceptions;

using System.Linq.Expressions;

using static Shared.Persistence.Abstractions.Constants.Enums;
using static Shared.Persistence.Constants.Enums;

namespace Shared.Persistence.Repositories;

public abstract class MongoRepository<TEntity> : IPersistenceNoSqlRepository<TEntity> where TEntity : class, IPersistentNoSql
{
    private readonly Lazy<IPersistenceReaderRepository<TEntity>> _reader;
    private readonly Lazy<IPersistenceWriterRepository<TEntity>> _writer;

    public IPersistenceReaderRepository<TEntity> Reader { get => _reader.Value; }
    public IPersistenceWriterRepository<TEntity> Writer { get => _writer.Value; }

    protected MongoRepository(ILogger<TEntity> logger, IMongoPersistenceContext context)
    {
        var objectId = base.GetHashCode();
        var initiator = $"Mongo repository of '{typeof(TEntity).Name}' by Id {objectId}";

        _reader = new Lazy<IPersistenceReaderRepository<TEntity>>(() => new MongoReaderRepository<TEntity>(context));
        _writer = new Lazy<IPersistenceWriterRepository<TEntity>>(() => new MongoWriterRepository<TEntity>(logger, context, initiator));
    }
}
internal sealed class MongoReaderRepository<TEntity> : IPersistenceReaderRepository<TEntity> where TEntity : IPersistentNoSql
{
    private readonly IMongoPersistenceContext _context;
    public MongoReaderRepository(IMongoPersistenceContext context) => _context = context;

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

    public async Task<T[]> GetProcessableAsync<T>(IProcessStep step, int limit, CancellationToken cToken = default) where T : class, IPersistentProcess, TEntity
    {
        Expression<Func<T, bool>> condition = x =>
            x.ProcessStepId == step.Id
            && x.ProcessStatusId == (int)ProcessStatuses.Ready;

        var updater = new Dictionary<ContextCommand, (string Name, object Value)>
        {
            { ContextCommand.Set, (nameof(IPersistentProcess.Updated), DateTime.UtcNow) },
            { ContextCommand.Set, (nameof(IPersistentProcess.ProcessStatusId), (int)ProcessStatuses.Processing) },
            { ContextCommand.Inc, (nameof(IPersistentProcess.ProcessAttempt), 1) }
        };

        return await _context.UpdateAsync(condition, updater, cToken);
    }
    public async Task<T[]> GetUnprocessableAsync<T>(IProcessStep step, int limit, DateTime updateTime, int maxAttempts, CancellationToken cToken = default) where T : class, IPersistentProcess, TEntity
    {
        Expression<Func<T, bool>> condition = x =>
            x.ProcessStepId == step.Id
            && ((x.ProcessStatusId == (int)ProcessStatuses.Processing && x.Updated < updateTime) || (x.ProcessStatusId == (int)ProcessStatuses.Error))
            && (x.ProcessAttempt < maxAttempts);

        var updater = new Dictionary<ContextCommand, (string Name, object Value)>
        {
            { ContextCommand.Set, (nameof(IPersistentProcess.Updated), DateTime.UtcNow) },
            { ContextCommand.Set, (nameof(IPersistentProcess.ProcessStatusId), (int)ProcessStatuses.Processing) },
            { ContextCommand.Inc, (nameof(IPersistentProcess.ProcessAttempt), 1) }
        };

        return await _context.UpdateAsync(condition, updater, cToken);
    }
}
internal sealed class MongoWriterRepository<TEntity> : IPersistenceWriterRepository<TEntity> where TEntity : IPersistentNoSql
{
    private readonly ILogger _logger;
    private readonly IMongoPersistenceContext _context;
    private readonly string _initiator;

    public MongoWriterRepository(ILogger logger, IMongoPersistenceContext context, string initiator)
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
    public async Task CreateRangeAsync<T>(IReadOnlyCollection<T> entities, CancellationToken cToken = default) where T : class, TEntity
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
            await CreateRangeAsync(entities, cToken);
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

    public async Task SaveProcessableAsync<T>(IProcessStep? step, IEnumerable<T> entities, CancellationToken cToken = default) where T : class, TEntity, IPersistentProcess
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

            throw new SharedPersistenceException(nameof(MongoWriterRepository<TEntity>), nameof(SaveProcessableAsync), new(exception));
        }
    }
}
