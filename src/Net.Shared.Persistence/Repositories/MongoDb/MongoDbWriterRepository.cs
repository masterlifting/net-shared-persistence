using System.Linq.Expressions;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Net.Shared.Models.Domain;
using Net.Shared.Persistence.Abstractions.Entities;
using Net.Shared.Persistence.Abstractions.Entities.Catalogs;
using Net.Shared.Persistence.Abstractions.Repositories.NoSql;
using Net.Shared.Persistence.Contexts;
using Net.Shared.Persistence.Models.Exceptions;
using static Net.Shared.Persistence.Models.Constants.Enums;

namespace Net.Shared.Persistence.Repositories.MongoDb;

public sealed class MongoDbWriterRepository<TEntity> : IPersistenceNoSqlWriterRepository<TEntity> where TEntity : class, IPersistentNoSql
{
    private readonly ILogger _logger;
    private readonly MongoDbContext _context;
    private readonly string _repositoryInfo;

    public MongoDbWriterRepository(ILogger<MongoDbWriterRepository<TEntity>> logger, MongoDbContext context)
    {
        _logger = logger;
        _context = context;
        _repositoryInfo = $"MongoDb repository {GetHashCode()} of the '{typeof(TEntity).Name}'";
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

    public async Task SetProcessableData<T>(IPersistentProcessStep? step, IEnumerable<T> entities, CancellationToken cToken = default) where T : class, TEntity, IPersistentProcess
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
