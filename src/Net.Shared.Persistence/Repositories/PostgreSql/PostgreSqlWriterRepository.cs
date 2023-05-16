﻿using System.Linq.Expressions;

using Microsoft.Extensions.Logging;
using Net.Shared.Extensions.Logging;

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

        _logger.Debug($"The entity '{typeof(T).Name}' was created by repository '{_repositoryInfo}'.");
    }
    public async Task CreateMany<T>(IReadOnlyCollection<T> entities, CancellationToken cToken) where T : class, IPersistentSql
    {
        if (!entities.Any())
        {
            _logger.Warning($"The entities '{typeof(T).Name}' weren't created by repository '{_repositoryInfo}' because the collection is empty.");
            return;
        }

        await _context.CreateMany(entities, cToken);

        _logger.Debug($"The entities '{typeof(T).Name}' were created by repository '{_repositoryInfo}'. Items count: {entities.Count}.");
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

    public async Task<T[]> Update<T>(Expression<Func<T, bool>> filter, Action<T> updater, PersistenceOptions? options, CancellationToken cToken) where T : class, IPersistentSql
    {
        var entities = await _context.Update(filter, updater, options, cToken);

        _logger.Debug($"The entities '{typeof(T).Name}' were updated by repository '{_repositoryInfo}'. Items count: {entities.Length}.");

        return entities;
    }
    public async Task<Result<T>> TryUpdate<T>(Expression<Func<T, bool>> filter, Action<T> updaters, PersistenceOptions? options, CancellationToken cToken) where T : class, IPersistentSql
    {
        try
        {
            var entities = await Update(filter, updaters, options, cToken);

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

    public async Task Update<T>(Expression<Func<T, bool>> filter, IEnumerable<T> data, PersistenceOptions? options, CancellationToken cToken) where T : class, IPersistentSql
    {
        await _context.Update(filter, data, options, cToken);

        _logger.Debug($"The entities '{typeof(T).Name}' were updated by repository '{_repositoryInfo}'. Items count: {data.Count()}.");
    }
    public async Task<Result<T>> TryUpdate<T>(Expression<Func<T, bool>> filter, IEnumerable<T> data, PersistenceOptions? options, CancellationToken cToken) where T : class, IPersistentSql
    {
        try
        {
            await Update(filter, data, options, cToken);
            return new Result<T>(data);
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

        _logger.Debug($"The entity '{typeof(T).Name}' was updated by repository '{_repositoryInfo}'.");

        return result;
    }
    public Task UpdateMany<T>(IEnumerable<T> entities, CancellationToken cToken) where T : class, IPersistentSql
    {
        var result = _context.UpdateMany(entities, cToken);

        _logger.Debug($"The entities '{typeof(T).Name}' were updated by repository '{_repositoryInfo}'. Items count: {entities.Count()}.");

        return result;
    }

    public async Task<T[]> Delete<T>(Expression<Func<T, bool>> filter, CancellationToken cToken) where T : class, IPersistentSql
    {
        var entities = await _context.Delete(filter, cToken);

        _logger.Debug($"The entities '{typeof(T).Name}' were deleted by repository '{_repositoryInfo}'. Items count: {entities.Length}.");

        return entities;
    }
    public async Task<Result<T>> TryDelete<T>(Expression<Func<T, bool>> filter, CancellationToken cToken) where T : class, IPersistentSql
    {
        try
        {
            var entities = await Delete(filter, cToken);

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
   
    public Task DeleteOne<T>(T entity, CancellationToken cToken) where T : class, IPersistentSql
    {
        var result = _context.DeleteOne(entity, cToken);
        _logger.Debug($"The entity '{typeof(T).Name}' was deleted by repository '{_repositoryInfo}'.");
        return result;
    }
    public Task DeleteMany<T>(IEnumerable<T> entities, CancellationToken cToken) where T : class, IPersistentSql
    {
        var result = _context.DeleteMany(entities, cToken);
        _logger.Debug($"The entities '{typeof(T).Name}' were deleted by repository '{_repositoryInfo}'. Items count: {entities.Count()}.");
        return result;
    }
    #endregion
}
