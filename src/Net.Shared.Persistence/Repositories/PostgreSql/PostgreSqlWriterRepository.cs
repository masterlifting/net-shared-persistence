using Microsoft.Extensions.Logging;

using Net.Shared.Extensions;
using Net.Shared.Models.Domain;
using Net.Shared.Persistence.Abstractions.Contexts;
using Net.Shared.Persistence.Abstractions.Entities;
using Net.Shared.Persistence.Abstractions.Repositories.Sql;
using Net.Shared.Persistence.Contexts;
using Net.Shared.Persistence.Models.Contexts;
using Net.Shared.Persistence.Models.Exceptions;

namespace Net.Shared.Persistence.Repositories.PostgreSql;

public sealed class PostgreSqlWriterRepository : IPersistenceSqlWriterRepository
{
    public PostgreSqlWriterRepository(ILogger<PostgreSqlWriterRepository> logger, PostgreSqlContext context)
    {
        _logger = logger;
        _context = context;
        Context = context;
        _repositoryInfo = $"PostgreSql {GetHashCode()}";
    }

    #region PRIVATE FIELDS
    private readonly ILogger _logger;
    private readonly PostgreSqlContext _context;
    private readonly string _repositoryInfo;
    #endregion

    #region PUBLIC PROPERTIES
    public IPersistenceSqlContext Context { get; }
    #endregion

    #region PUBLIC METHODS
    public async Task CreateOne<T>(T entity, CancellationToken cToken) where T : class, IPersistentSql
    {
        await _context.CreateOne(entity, cToken);

        _logger.Debug($"<{typeof(T).Name}> was created by repository '{_repositoryInfo}'.");
    }
    public async Task CreateMany<T>(IReadOnlyCollection<T> entities, CancellationToken cToken) where T : class, IPersistentSql
    {
        if (!entities.Any())
        {
            _logger.Warning($"<{typeof(T).Name}> weren't created by repository '{_repositoryInfo}' because the collection is empty.");
            return;
        }

        await _context.CreateMany(entities, cToken);

        _logger.Debug($"<{typeof(T).Name}> were created by repository '{_repositoryInfo}'. Count: {entities.Count}.");
    }
    public async Task<Result<T>> TryCreateOne<T>(T entity, CancellationToken cToken) where T : class, IPersistentSql
    {
        try
        {
            await CreateOne(entity, cToken);
            return new Result<T>(new[] { entity });
        }
        catch (PersistenceException exception)
        {
            return new Result<T>(exception);
        }
        catch (Exception exception)
        {
            return new Result<T>(new PersistenceException(exception));
        }
    }
    public async Task<Result<T>> TryCreateMany<T>(IReadOnlyCollection<T> entities, CancellationToken cToken) where T : class, IPersistentSql
    {
        try
        {
            await CreateMany(entities, cToken);
            return new Result<T>(entities);
        }
        catch (PersistenceException exception)
        {
            return new Result<T>(exception);
        }
        catch (Exception exception)
        {
            return new Result<T>(new PersistenceException(exception));
        }
    }

    public async Task<T[]> Update<T>(PersistenceUpdateOptions<T> options, CancellationToken cToken) where T : class, IPersistentSql
    {
        var entities = await _context.Update(options, cToken);

        _logger.Debug($"<{typeof(T).Name}> were updated by repository '{_repositoryInfo}'. Count: {entities.Length}.");

        return entities;
    }
    public async Task<Result<T>> TryUpdate<T>(PersistenceUpdateOptions<T> options, CancellationToken cToken) where T : class, IPersistentSql
    {
        try
        {
            var entities = await Update(options, cToken);

            return new Result<T>(entities);
        }
        catch (PersistenceException exception)
        {
            return new Result<T>(exception);
        }
        catch (Exception exception)
        {
            return new Result<T>(new PersistenceException(exception));
        }
    }

    public Task UpdateOne<T>(T entity, CancellationToken cToken) where T : class, IPersistentSql
    {
        var result = _context.UpdateOne(entity, cToken);

        _logger.Debug($"<{typeof(T).Name}> was updated by repository '{_repositoryInfo}'.");

        return result;
    }
    public Task UpdateMany<T>(IEnumerable<T> entities, CancellationToken cToken) where T : class, IPersistentSql
    {
        var result = _context.UpdateMany(entities, cToken);

        _logger.Debug($"<{typeof(T).Name}> were updated by repository '{_repositoryInfo}'. Count: {entities.Count()}.");

        return result;
    }

    public async Task<long> Delete<T>(PersistenceQueryOptions<T> options, CancellationToken cToken) where T : class, IPersistentSql
    {
        var count = await _context.Delete(options, cToken);

        _logger.Debug($"<{typeof(T).Name}> were deleted by repository '{_repositoryInfo}'. Count: {count}.");

        return count;
    }
    public async Task<Result<long>> TryDelete<T>(PersistenceQueryOptions<T> options, CancellationToken cToken) where T : class, IPersistentSql
    {
        try
        {
            var count = await Delete(options, cToken);

            return new Result<long>(count);
        }
        catch (PersistenceException exception)
        {
            return new Result<long>(exception);
        }
        catch (Exception exception)
        {
            return new Result<long>(new PersistenceException(exception));
        }
    }

    public Task DeleteOne<T>(T entity, CancellationToken cToken) where T : class, IPersistentSql
    {
        var result = _context.DeleteOne(entity, cToken);
        _logger.Debug($"<{typeof(T).Name}> was deleted by repository '{_repositoryInfo}'.");
        return result;
    }
    public Task DeleteMany<T>(IEnumerable<T> entities, CancellationToken cToken) where T : class, IPersistentSql
    {
        var result = _context.DeleteMany(entities, cToken);
        _logger.Debug($"<{typeof(T).Name}> were deleted by repository '{_repositoryInfo}'. Count: {entities.Count()}.");
        return result;
    }
    #endregion
}
