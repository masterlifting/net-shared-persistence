using Microsoft.Extensions.Logging;

using Net.Shared.Extensions.Logging;
using Net.Shared.Abstractions.Models.Data;
using Net.Shared.Persistence.Abstractions.Interfaces.Contexts;
using Net.Shared.Persistence.Abstractions.Interfaces.Entities;
using Net.Shared.Persistence.Abstractions.Interfaces.Repositories.Sql;
using Net.Shared.Persistence.Abstractions.Models.Contexts;
using Net.Shared.Persistence.Contexts;

namespace Net.Shared.Persistence.Repositories.PostgreSql;

public sealed class PostgreSqlWriterRepository : IPersistenceSqlWriterRepository
{
    public PostgreSqlWriterRepository(ILogger<PostgreSqlWriterRepository> logger, PostgreSqlContext context)
    {
        _log = logger;
        _context = context;
        Context = context;
        _repositoryInfo = $"PostgreSql {GetHashCode()}";
    }

    #region PRIVATE FIELDS
    private readonly ILogger _log;
    private readonly PostgreSqlContext _context;
    private readonly string _repositoryInfo;
    #endregion

    #region PUBLIC PROPERTIES
    public IPersistenceSqlContext Context { get; }
    #endregion

    #region PUBLIC METHODS
    public async Task CreateOne<T>(T entity, CancellationToken cToken) where T : class, IPersistent, IPersistentSql
    {
        await _context.CreateOne(entity, cToken);

        _log.Debug($"<{typeof(T).Name}> was created by repository '{_repositoryInfo}'.");
    }
    public async Task CreateMany<T>(IReadOnlyCollection<T> entities, CancellationToken cToken) where T : class, IPersistent, IPersistentSql
    {
        if (!entities.Any())
        {
            _log.Warn($"<{typeof(T).Name}> weren't created by repository '{_repositoryInfo}' because the collection is empty.");
            return;
        }

        await _context.CreateMany(entities, cToken);

        _log.Debug($"<{typeof(T).Name}> were created by repository '{_repositoryInfo}'. Count: {entities.Count}.");
    }
    public async Task<Result<T>> TryCreateOne<T>(T entity, CancellationToken cToken) where T : class, IPersistent, IPersistentSql
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
    public async Task<Result<T>> TryCreateMany<T>(IReadOnlyCollection<T> entities, CancellationToken cToken) where T : class, IPersistent, IPersistentSql
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

    public async Task<T[]> Update<T>(PersistenceUpdateOptions<T> options, CancellationToken cToken) where T : class, IPersistent, IPersistentSql
    {
        var entities = await _context.Update(options, cToken);

        _log.Debug($"<{typeof(T).Name}> were updated by repository '{_repositoryInfo}'. Count: {entities.Length}.");

        return entities;
    }
    public async Task<Result<T>> TryUpdate<T>(PersistenceUpdateOptions<T> options, CancellationToken cToken) where T : class, IPersistent, IPersistentSql
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

    public Task UpdateOne<T>(T entity, CancellationToken cToken) where T : class, IPersistentSql
    {
        var result = _context.UpdateOne(entity, cToken);

        _log.Debug($"<{typeof(T).Name}> was updated by repository '{_repositoryInfo}'.");

        return result;
    }
    public Task UpdateMany<T>(IEnumerable<T> entities, CancellationToken cToken) where T : class, IPersistentSql
    {
        var result = _context.UpdateMany(entities, cToken);

        _log.Debug($"<{typeof(T).Name}> were updated by repository '{_repositoryInfo}'. Count: {entities.Count()}.");

        return result;
    }

    public async Task<long> Delete<T>(PersistenceQueryOptions<T> options, CancellationToken cToken) where T : class, IPersistent, IPersistentSql
    {
        var count = await _context.Delete(options, cToken);

        _log.Debug($"<{typeof(T).Name}> were deleted by repository '{_repositoryInfo}'. Count: {count}.");

        return count;
    }
    public async Task<Result<long>> TryDelete<T>(PersistenceQueryOptions<T> options, CancellationToken cToken) where T : class, IPersistent, IPersistentSql
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

    public Task DeleteOne<T>(T entity, CancellationToken cToken) where T : class, IPersistentSql
    {
        var result = _context.DeleteOne(entity, cToken);
        _log.Debug($"<{typeof(T).Name}> was deleted by repository '{_repositoryInfo}'.");
        return result;
    }
    public Task DeleteMany<T>(IEnumerable<T> entities, CancellationToken cToken) where T : class, IPersistentSql
    {
        var result = _context.DeleteMany(entities, cToken);
        _log.Debug($"<{typeof(T).Name}> were deleted by repository '{_repositoryInfo}'. Count: {entities.Count()}.");
        return result;
    }
    #endregion
}
