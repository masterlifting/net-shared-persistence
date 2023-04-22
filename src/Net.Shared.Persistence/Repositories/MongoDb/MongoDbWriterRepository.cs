using System.Linq.Expressions;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Net.Shared.Extensions.Logging;
using Net.Shared.Models.Domain;
using Net.Shared.Persistence.Abstractions.Contexts;
using Net.Shared.Persistence.Abstractions.Entities;
using Net.Shared.Persistence.Abstractions.Repositories.NoSql;
using Net.Shared.Persistence.Contexts;

namespace Net.Shared.Persistence.Repositories.MongoDb;

public sealed class MongoDbWriterRepository : IPersistenceNoSqlWriterRepository
{
    public MongoDbWriterRepository(ILogger<MongoDbWriterRepository> logger, MongoDbContext context)
    {
        _logger = logger;
        _context = context;
        Context = context;
        _repositoryInfo = $"MongoDb repository {GetHashCode()}.'";
    }

    #region PRIVATE FIELDS
    private readonly ILogger _logger;
    private readonly MongoDbContext _context;
    private readonly string _repositoryInfo;
    #endregion

    #region PUBLIC PROPERTIES
    public IPersistenceNoSqlContext Context { get; }
    #endregion

    #region PUBLIC METHODS
    public async Task CreateOne<T>(T entity, CancellationToken cToken) where T : class, IPersistentNoSql
    {
        await _context.CreateOne(entity, cToken);

        _logger.Trace($"The entity {entity} was created by {_repositoryInfo}.");
    }
    public async Task CreateMany<T>(IReadOnlyCollection<T> entities, CancellationToken cToken) where T : class, IPersistentNoSql
    {
        if (!entities.Any())
        {
            _logger.Trace($"The entities {entities} were not created by {_repositoryInfo} because the collection is empty.");
            return;
        }

        await _context.CreateMany(entities, cToken);

        _logger.Trace($"The entities {entities} were created by {_repositoryInfo}.");
    }

    public async Task<Result<T>> TryCreateOne<T>(T entity, CancellationToken cToken) where T : class, IPersistentNoSql
    {
        try
        {
            await CreateOne(entity, cToken);
            return new Result<T>(new[] { entity });
        }
        catch (Exception exception)
        {
            return new Result<T>(exception);
        }
    }
    public async Task<Result<T>> TryCreateMany<T>(IReadOnlyCollection<T> entities, CancellationToken cToken) where T : class, IPersistentNoSql
    {
        try
        {
            await CreateMany(entities, cToken);
            return new Result<T>(entities);
        }
        catch (Exception exception)
        {
            return new Result<T>(exception);
        }
    }

    public Task<T[]> Update<T>(Expression<Func<T, bool>> filter, Action<T> updaters, CancellationToken cToken) where T : class, IPersistentNoSql
    {
        throw new NotImplementedException();
    }
    public Task<Result<T>> TryUpdate<T>(Expression<Func<T, bool>> filter, Action<T> updaters, CancellationToken cToken) where T : class, IPersistentNoSql
    {
        throw new NotImplementedException();
    }
    public async Task<T[]> Update<T>(Expression<Func<T, bool>> filter, T entity, CancellationToken cToken) where T : class, IPersistentNoSql
    {
        var entities = await _context.Update(filter, entity, cToken);

        _logger.Trace($"The entities {entities} were updated by {_repositoryInfo}.");

        return entities;
    }
    public async Task<Result<T>> TryUpdate<T>(Expression<Func<T, bool>> filter, T entity, CancellationToken cToken) where T : class, IPersistentNoSql
    {
        try
        {
            var entities = await Update(filter, entity, cToken);
            return new Result<T>(entities);
        }
        catch (Exception exception)
        {
            return new Result<T>(exception);
        }
    }

    public async Task<T[]> Delete<T>(Expression<Func<T, bool>> filter, CancellationToken cToken) where T : class, IPersistentNoSql
    {
        var entities = await _context.Delete(filter, cToken);

        _logger.Trace($"The entities {entities} were deleted by {_repositoryInfo}.");

        return entities;
    }
    public async Task<Result<T>> TryDelete<T>(Expression<Func<T, bool>> filter, CancellationToken cToken) where T : class, IPersistentNoSql
    {
        try
        {
            var entities = await Delete(filter, cToken);
            return new Result<T>(entities);
        }
        catch (Exception exception)
        {
            return new Result<T>(exception);
        }
    }
    #endregion
}
