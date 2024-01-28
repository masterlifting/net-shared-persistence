using Microsoft.Extensions.Logging;

using Net.Shared.Extensions.Logging;
using Net.Shared.Abstractions.Models.Data;
using Net.Shared.Persistence.Abstractions.Interfaces.Entities;
using Net.Shared.Persistence.Abstractions.Models.Contexts;
using Net.Shared.Persistence.Contexts;
using Net.Shared.Persistence.Abstractions.Interfaces.Repositories;

namespace Net.Shared.Persistence.Repositories.PostgreSql;

public class PostgreSqlWriterRepository<TEntity> : IPersistenceWriterRepository<TEntity> where TEntity : IPersistentSql
{
    private readonly ILogger _log;
    private readonly PostgreSqlContext _context;

    private readonly string _repository;
    public PostgreSqlWriterRepository(ILogger<PostgreSqlWriterRepository<TEntity>> logger, PostgreSqlContext context)
    {
        _log = logger;
        _context = context;
        _repository = nameof(PostgreSqlWriterRepository<TEntity>) + ' ' + GetHashCode();

        _log.Warn(_repository);
    }

    public async Task CreateOne<T>(T entity, CancellationToken cToken) where T : class, TEntity
    {
        await _context.CreateOne(entity, cToken);

        _log.Debug($"<Created by '{_repository}'.");
    }
    public async Task<Result<T>> TryCreateOne<T>(T entity, CancellationToken cToken) where T : class, TEntity
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

    public async Task CreateMany<T>(IReadOnlyCollection<T> entities, CancellationToken cToken) where T : class, TEntity
    {
        await _context.CreateMany(entities, cToken);

        _log.Debug($"<Created by '{_repository}'. Count: {entities.Count}.");
    }
    public async Task<Result<T>> TryCreateMany<T>(IReadOnlyCollection<T> entities, CancellationToken cToken) where T : class, TEntity
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

    public async Task<T[]> Update<T>(PersistenceUpdateOptions<T> options, CancellationToken cToken) where T : class, TEntity
    {
        var entities = await _context.Update(options, cToken);

        _log.Debug($"<Updated by '{_repository}'. Count: {entities.Length}.");

        return entities;
    }
    public async Task<Result<T>> TryUpdate<T>(PersistenceUpdateOptions<T> options, CancellationToken cToken) where T : class, TEntity
    {
        try
        {
            var entities = await Update(options, cToken);
            return new Result<T>(entities);
        }
        catch (Exception exception)
        {
            return new Result<T>(exception);
        }
    }

    public async Task<long> Delete<T>(PersistenceQueryOptions<T> options, CancellationToken cToken) where T : class, TEntity
    {
        var count = await _context.Delete(options, cToken);

        _log.Debug($"<Deleted by '{_repository}'. Count: {count}.");

        return count;
    }
    public async Task<Result<long>> TryDelete<T>(PersistenceQueryOptions<T> options, CancellationToken cToken) where T : class, TEntity
    {
        try
        {
            var count = await Delete(options, cToken);
            return new Result<long>(count);
        }
        catch (Exception exception)
        {
            return new Result<long>(exception);
        }
    }

    #region Specialized API
    
    public Task UpdateOne<T>(T entity, CancellationToken cToken) where T : class, TEntity
    {
        var result = _context.UpdateOne(entity, cToken);

        _log.Debug($"<Updated by '{_repository}'.");

        return result;
    }
    public Task UpdateMany<T>(IEnumerable<T> entities, CancellationToken cToken) where T : class, TEntity
    {
        var result = _context.UpdateMany(entities, cToken);
        
        _log.Debug($"<Updated by '{_repository}'. Count: {entities.Count()}.");
        
        return result;
    }

    public Task DeleteOne<T>(T entity, CancellationToken cToken) where T : class, TEntity
    {
        var result = _context.DeleteOne(entity, cToken);
        
        _log.Debug($"<Deleted by '{_repository}'.");
        
        return result;
    }
    public Task DeleteMany<T>(IEnumerable<T> entities, CancellationToken cToken) where T : class, TEntity
    {
        var result = _context.DeleteMany(entities, cToken);
        
        _log.Debug($"<Deleted by ' {_repository}'. Count: {entities.Count()}.");
        
        return result;
    }
    
    #endregion
}
