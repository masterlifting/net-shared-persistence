﻿using System.Linq.Expressions;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Net.Shared.Persistence.Abstractions.Contexts;
using Net.Shared.Persistence.Abstractions.Entities;
using Net.Shared.Persistence.Models.Contexts;
using Net.Shared.Persistence.Models.Exceptions;
using Net.Shared.Persistence.Models.Settings.Connections;

namespace Net.Shared.Persistence.Contexts;

public abstract class PostgreSqlContext : DbContext, IPersistenceSqlContext
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly PostgreSqlConnection _connectionSettings;
    private bool _isExternalTransaction;

    protected PostgreSqlContext(ILoggerFactory loggerFactory, PostgreSqlConnection connectionSettings)
    {
        _loggerFactory = loggerFactory;
        _connectionSettings = connectionSettings;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder builder)
    {
        builder.UseLoggerFactory(_loggerFactory);
        builder.UseNpgsql(_connectionSettings.ConnectionString);
        base.OnConfiguring(builder);
    }
    public IQueryable<T> SetEntity<T>() where T : class, IPersistentSql => Set<T>();

    public string GetTableName<T>() where T : class, IPersistentSql =>
        Model.FindEntityType(typeof(T))?.ShortName() ?? throw new PersistenceException($"Searching a table name {typeof(T).Name} was not found.");

    public IQueryable<T> GetQueryFromRaw<T>(FormattableString query, CancellationToken cToken = default) where T : class, IPersistentSql =>
        Set<T>().FromSqlRaw(query.Format);

    public Task<T?> FindById<T>(object[] id, CancellationToken cToken) where T : class, IPersistentSql =>
        Set<T>().FindAsync(id, cToken).AsTask();
    public Task<T?> FindById<T>(object id, CancellationToken cToken) where T : class, IPersistentSql =>
        Set<T>().FindAsync(id, cToken).AsTask();

    public Task<T[]> FindAll<T>(CancellationToken cToken) where T : class, IPersistentSql => Set<T>().ToArrayAsync(cToken);
    public Task<T[]> FindMany<T>(Expression<Func<T, bool>> filter, CancellationToken cToken = default) where T : class, IPersistentSql =>
        Set<T>().Where(filter).ToArrayAsync(cToken);
    public Task<T?> FindFirst<T>(Expression<Func<T, bool>> filter, CancellationToken cToken = default) where T : class, IPersistentSql =>
        Set<T>().FirstOrDefaultAsync(filter, cToken);
    public Task<T?> FindSingle<T>(Expression<Func<T, bool>> filter, CancellationToken cToken = default) where T : class, IPersistentSql =>
        Set<T>().SingleOrDefaultAsync(filter, cToken);

    public async Task CreateOne<T>(T entity, CancellationToken cToken = default) where T : class, IPersistentSql
    {
        try
        {
            await Set<T>().AddAsync(entity, cToken);
            await SaveChangesAsync(cToken);
        }
        catch (Exception exception)
        {
            throw new PersistenceException(exception);
        }
    }
    public async Task CreateMany<T>(IReadOnlyCollection<T> entities, CancellationToken cToken = default) where T : class, IPersistentSql
    {
        try
        {
            await Set<T>().AddRangeAsync(entities, cToken);
            await SaveChangesAsync(cToken);
        }
        catch (Exception exception)
        {
            throw new PersistenceException(exception);
        }
    }

    public async Task<T[]> Update<T>(Expression<Func<T, bool>> filter, Action<T> updater, PersistenceOptions? options, CancellationToken cToken) where T : class, IPersistentSql
    {
        try
        {
            if (!_isExternalTransaction && Database.CurrentTransaction is null)
                await Database.BeginTransactionAsync(cToken);

            var query = Set<T>().Where(filter);

            if(options is not null)
            {
                if(options.Limit > 0)
                    query = query.Take(options.Limit);
                
                if(options.OrderSelector is not null)
                {
                    var parameter = Expression.Parameter(typeof(T), "x");
                    var property = Expression.Property(parameter, options.OrderSelector);
                    var lambda = Expression.Lambda<Func<T, object>>(property, parameter);
                    
                    query = options.OrderIsAsc ? query.OrderBy(lambda) : query.OrderByDescending(lambda);
                }
            }

            var entities = await query.ToArrayAsync(cToken);

            if (!entities.Any())
                return entities;

            foreach (var entity in entities)
                updater(entity);

            await SaveChangesAsync(cToken);

            if (!_isExternalTransaction && Database.CurrentTransaction is not null)
                await Database.CurrentTransaction.CommitAsync(cToken);

            return entities;
        }
        catch (Exception exception)
        {
            if (Database.CurrentTransaction is not null)
                await Database.CurrentTransaction.RollbackAsync(cToken);

            throw new PersistenceException(exception);
        }
        finally
        {
            if (!_isExternalTransaction && Database.CurrentTransaction is not null)
                await Database.CurrentTransaction.DisposeAsync();
        }
    }
    public async Task Update<T>(Expression<Func<T, bool>> filter, IEnumerable<T> data, PersistenceOptions? options, CancellationToken cToken) where T : class, IPersistentSql
    {
        try
        {
            if (!_isExternalTransaction && Database.CurrentTransaction is null)
                await Database.BeginTransactionAsync(cToken);

            //TOTO: Implement improves for update many

           Set<T>().UpdateRange(data);
            await SaveChangesAsync(cToken);

            if (!_isExternalTransaction && Database.CurrentTransaction is not null)
                await Database.CurrentTransaction.CommitAsync(cToken);
        }
        catch (Exception exception)
        {
            if (Database.CurrentTransaction is not null)
                await Database.CurrentTransaction.RollbackAsync(cToken);

            throw new PersistenceException(exception);
        }
        finally
        {
            if (!_isExternalTransaction && Database.CurrentTransaction is not null)
                await Database.CurrentTransaction.DisposeAsync();
        }
    }
    
    public async Task UpdateOne<T>(T entity, CancellationToken cToken) where T : class, IPersistentSql
    {
        try
        {
            base.Set<T>().Update(entity);
            await SaveChangesAsync(cToken);
        }
        catch (Exception exception)
        {
            throw new PersistenceException(exception);
        }
    }
    public async Task UpdateMany<T>(IEnumerable<T> entities, CancellationToken cToken) where T : class, IPersistentSql
    {
        try
        {
            base.Set<T>().UpdateRange(entities);
            await SaveChangesAsync(cToken);
        }
        catch (Exception exception)
        {
            throw new PersistenceException(exception);
        }
    }

    public async Task<T[]> Delete<T>(Expression<Func<T, bool>> filter, CancellationToken cToken = default) where T : class, IPersistentSql
    {
        try
        {
            if (!_isExternalTransaction && Database.CurrentTransaction is null)
                await Database.BeginTransactionAsync(cToken);

            var entities = await FindMany(filter, cToken);

            if (!entities.Any())
                return entities;

            await DeleteMany(entities, cToken);

            if (!_isExternalTransaction && Database.CurrentTransaction is not null)
                await Database.CurrentTransaction.CommitAsync(cToken);

            return entities;
        }
        catch (Exception exception)
        {
            if (Database.CurrentTransaction is not null)
                await Database.CurrentTransaction.RollbackAsync(cToken);

            throw new PersistenceException(exception);
        }
        finally
        {
            if (!_isExternalTransaction && Database.CurrentTransaction is not null)
                await Database.CurrentTransaction.DisposeAsync();
        }
    }

    public async Task DeleteOne<T>(T entity, CancellationToken cToken) where T : class, IPersistentSql
    {
        try
        {
            base.Set<T>().Remove(entity);
            await SaveChangesAsync(cToken);
        }
        catch (Exception exception)
        {
            throw new PersistenceException(exception);
        }
    }
    public async Task DeleteMany<T>(IEnumerable<T> entities, CancellationToken cToken) where T : class, IPersistentSql
    {
        try
        {
            base.Set<T>().RemoveRange(entities);
            await SaveChangesAsync(cToken);
        }
        catch (Exception exception)
        {
            throw new PersistenceException(exception);
        }
    }

    public Task StartTransaction(CancellationToken cToken = default)
    {
        if (Database.CurrentTransaction is not null)
        {
            return Task.CompletedTask;
        }
        else
        {
            _isExternalTransaction = true;
            return Database.BeginTransactionAsync(cToken);
        }
    }
    public async Task CommitTransaction(CancellationToken cToken = default)
    {
        if (Database.CurrentTransaction is null)
            throw new PersistenceException("No transaction to commit.");

        try
        {
            await Database.CurrentTransaction.CommitAsync(cToken);
        }
        catch (Exception exception)
        {
            throw new PersistenceException(exception);
        }
        finally
        {
            _isExternalTransaction = false;
            await Database.CurrentTransaction.DisposeAsync();
        }
    }
    public async Task RollbackTransaction(CancellationToken cToken = default)
    {
        if (Database.CurrentTransaction is null)
            throw new PersistenceException("No transaction to rollback.");

        try
        {
            await Database.CurrentTransaction.RollbackAsync(cToken);
        }
        catch (Exception exception)
        {
            throw new PersistenceException(exception);
        }
        finally
        {
            _isExternalTransaction = false;
            await Database.CurrentTransaction.DisposeAsync();
        }
    }
}
