using Microsoft.Extensions.Logging;

using MongoDB.Driver;

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

public abstract class MongoRepository<TEntity> : IPersistenceNoSqlRepository<TEntity> where TEntity : class, IPersistentNoSql
{
    private readonly Lazy<IPersistenceReaderRepository<TEntity>> _reader;
    private readonly Lazy<IPersistenceWriterRepository<TEntity>> _writer;

    public IPersistenceReaderRepository<TEntity> Reader { get => _reader.Value; }
    public IPersistenceWriterRepository<TEntity> Writer { get => _writer.Value; }

    protected MongoRepository(ILogger<TEntity> logger, IPersistenceMongoContext context)
    {
        var objectId = base.GetHashCode();
        var repositoryInfo = $"Mongo repository {objectId} of '{typeof(TEntity).Name}'";

        _reader = new Lazy<IPersistenceReaderRepository<TEntity>>(() => new MongoReaderRepository<TEntity>(context));
        _writer = new Lazy<IPersistenceWriterRepository<TEntity>>(() => new MongoWriterRepository<TEntity>(logger, context, repositoryInfo));
    }
}
internal sealed class MongoReaderRepository<TEntity> : IPersistenceReaderRepository<TEntity> where TEntity : class, IPersistentNoSql
{
    private readonly IPersistenceMongoContext _context;
    public MongoReaderRepository(IPersistenceMongoContext context) => _context = context;

    public Task<T?> FindSingle<T>(Expression<Func<T, bool>> filter, CancellationToken cToken = default) where T : class, TEntity =>
        _context.FindSingle(filter, cToken);
    public Task<T?> FindFirst<T>(Expression<Func<T, bool>> filter, CancellationToken cToken = default) where T : class, TEntity =>
        _context.FindFirst(filter, cToken);
    public Task<T[]> FindMany<T>(Expression<Func<T, bool>> filter, CancellationToken cToken = default) where T : class, TEntity =>
        _context.FindMany(filter, cToken);

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
    public async Task<T[]> GetUnprocessableData<T>(IPersistentProcessStep step, int limit, DateTime updateTime, int maxAttempts, CancellationToken cToken = default) where T : class, IPersistentProcess, TEntity
    {
        Expression<Func<T, bool>> condition = x =>
            x.ProcessStepId == step.Id
            && (x.ProcessStatusId == (int)ProcessStatuses.Processing && x.Updated < updateTime || x.ProcessStatusId == (int)ProcessStatuses.Error)
            && x.ProcessAttempt < maxAttempts;

        var updater = (T x) =>
        {
            x.Updated = DateTime.UtcNow;
            x.ProcessStatusId = (int)ProcessStatuses.Processing;
            x.ProcessAttempt++;
        };

        return await _context.Update(condition, updater, cToken);
    }
}
public sealed class MongoWriterRepository<TEntity> : IPersistenceWriterRepository<TEntity> where TEntity : class, IPersistentNoSql
{
    private readonly ILogger _logger;
    private readonly IPersistenceMongoContext _context;
    private readonly string _repositoryInfo;

    public MongoWriterRepository(ILogger logger, IPersistenceMongoContext context, string repositoryInfo)
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

    public Task<T[]> Update<T>(Expression<Func<T, bool>> filter, Action<T> updaters, CancellationToken cToken) where T : class, TEntity
    {
        throw new NotImplementedException();
    }
    public Task<TryResult<T[]>> TryUpdate<T>(Expression<Func<T, bool>> filter, Action<T> updaters, CancellationToken cToken) where T : class, TEntity
    {
        throw new NotImplementedException();
    }
    public async Task<T[]> Update<T>(Expression<Func<T, bool>> filter, T entity, CancellationToken cToken = default) where T : class, TEntity
    {
        var entities = await _context.Update(filter, entity, cToken);

        _logger.LogTrace(_repositoryInfo, Constants.Actions.Updated, Constants.Actions.Success, entities.Length);

        return entities;
    }
    public async Task<TryResult<T[]>> TryUpdate<T>(Expression<Func<T, bool>> filter, T entity, CancellationToken cToken = default) where T : class, TEntity
    {
        try
        {
            var entities = await Update(filter, entity, cToken);
            return new TryResult<T[]>(entities);
        }
        catch (Exception exception)
        {
            return new TryResult<T[]>(exception);
        }
    }

    public async Task<T[]> Delete<T>(Expression<Func<T, bool>> filter, CancellationToken cToken = default) where T : class, TEntity
    {
        var entities = await _context.Delete(filter, cToken);

        _logger.LogTrace(_repositoryInfo, Constants.Actions.Deleted, Constants.Actions.Success, entities.Length);

        return entities;
    }
    public async Task<TryResult<T[]>> TryDelete<T>(Expression<Func<T, bool>> filter, CancellationToken cToken = default) where T : class, TEntity
    {
        try
        {
            var entities = await Delete(filter, cToken);
            return new TryResult<T[]>(entities);
        }
        catch (Exception exception)
        {
            return new TryResult<T[]>(exception);
        }
    }

    public async Task SaveProcessableData<T>(IPersistentProcessStep? step, IEnumerable<T> entities, CancellationToken cToken = default) where T : class, TEntity, IPersistentProcess
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
