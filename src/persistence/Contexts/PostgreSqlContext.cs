using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Net.Shared.Persistence.Abstractions.Interfaces.Contexts;
using Net.Shared.Persistence.Abstractions.Interfaces.Entities;
using Net.Shared.Persistence.Abstractions.Models.Contexts;
using Net.Shared.Persistence.Abstractions.Models.Settings.Connections;

namespace Net.Shared.Persistence.Contexts;

public abstract class PostgreSqlContext : DbContext, IPersistenceContext<IPersistentSql>
{
    private readonly PostgreSqlConnectionSettings _connectionSettings;
    
    private int _isExternalTransactionValue = 0;
    private bool IsExternalTransaction
    {
        get => Interlocked.CompareExchange(ref _isExternalTransactionValue, 1, 1) == 1;
        set
        {
            if (value)
                Interlocked.CompareExchange(ref _isExternalTransactionValue, 1, 0);
            else
                Interlocked.CompareExchange(ref _isExternalTransactionValue, 0, 1);
        }
    }

    public IQueryable<T> GetQuery<T>() where T : class, IPersistent, IPersistentSql => Set<T>();

    protected PostgreSqlContext(ILogger _, PostgreSqlConnectionSettings connectionSettings)
    {
        _connectionSettings = connectionSettings;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder builder)
    {
        builder.UseNpgsql(_connectionSettings.ConnectionString);
        base.OnConfiguring(builder);
    }

    #region Specialized API
    public string GetTableName<T>() where T : class, IPersistentSql =>
        Model.FindEntityType(typeof(T))?.ShortName() ?? throw new InvalidOperationException($"Searching a table name {typeof(T).Name} was not found.");

    public IQueryable<T> GetQueryFromRaw<T>(FormattableString query, CancellationToken cToken) where T : class, IPersistentSql =>
        Set<T>().FromSqlRaw(query.Format);

    public Task<T?> FindById<T>(object[] id, CancellationToken cToken) where T : class, IPersistentSql =>
        Set<T>().FindAsync(id, cToken).AsTask();
    public Task<T?> FindById<T>(object id, CancellationToken cToken) where T : class, IPersistentSql =>
        Set<T>().FindAsync(id, cToken).AsTask();

    public async Task UpdateOne<T>(T entity, CancellationToken cToken) where T : class, IPersistentSql
    {
        base.Set<T>().Update(entity);
        await SaveChangesAsync(cToken);
    }
    public async Task UpdateMany<T>(IEnumerable<T> entities, CancellationToken cToken) where T : class, IPersistentSql
    {
        base.Set<T>().UpdateRange(entities);
        await SaveChangesAsync(cToken);
    }

    public async Task DeleteOne<T>(T entity, CancellationToken cToken) where T : class, IPersistentSql
    {
        base.Set<T>().Remove(entity);
        await SaveChangesAsync(cToken);
    }
    public async Task DeleteMany<T>(IEnumerable<T> entities, CancellationToken cToken) where T : class, IPersistentSql
    {
        base.Set<T>().RemoveRange(entities);
        await SaveChangesAsync(cToken);
    }
    #endregion

    public Task<bool> IsExists<T>(PersistenceQueryOptions<T> options, CancellationToken cToken) where T : class, IPersistent, IPersistentSql
    {
        var query = GetQuery<T>();
        options.BuildQuery(ref query);
        return query.AnyAsync(cToken);
    }

    public Task<T?> FindFirst<T>(PersistenceQueryOptions<T> options, CancellationToken cToken) where T : class, IPersistent, IPersistentSql
    {
        var query = GetQuery<T>();
        options.Take = 1;
        options.BuildQuery(ref query);
        return query.FirstOrDefaultAsync(cToken);
    }
    public Task<T?> FindSingle<T>(PersistenceQueryOptions<T> options, CancellationToken cToken) where T : class, IPersistent, IPersistentSql
    {
        var query = GetQuery<T>();
        options.Take = 2;
        options.BuildQuery(ref query);
        return query.SingleOrDefaultAsync(cToken);
    }

    public Task<T[]> FindMany<T>(PersistenceQueryOptions<T> options, CancellationToken cToken) where T : class, IPersistent, IPersistentSql
    {
        var query = GetQuery<T>();
        options.BuildQuery(ref query);
        return query.ToArrayAsync(cToken);
    }
    public Task<TResult[]> FindMany<T, TResult>(PersistenceSelectorOptions<T, TResult> options, CancellationToken cToken) where T : class, IPersistent, IPersistentSql
    {
        var query = GetQuery<T>();
        options.QueryOptions.BuildQuery(ref query);
        return query.Select(options.Selector).ToArrayAsync(cToken);
    }

    public async Task CreateOne<T>(T entity, CancellationToken cToken) where T : class, IPersistent, IPersistentSql
    {
        await Set<T>().AddAsync(entity, cToken);
        await SaveChangesAsync(cToken);
    }
    public async Task CreateMany<T>(IReadOnlyCollection<T> entities, CancellationToken cToken) where T : class, IPersistent, IPersistentSql
    {
        await Set<T>().AddRangeAsync(entities, cToken);
        await SaveChangesAsync(cToken);
    }

    public async Task<T[]> Update<T>(PersistenceUpdateOptions<T> options, CancellationToken cToken) where T : class, IPersistent, IPersistentSql
    {
        try
        {
            if (!IsExternalTransaction && Database.CurrentTransaction is null)
                await Database.BeginTransactionAsync(cToken);

            T[] entities;

            if (options.Data is not null)
            {
                entities = options.Data;
            }
            else
            {
                var query = GetQuery<T>();

                options.QueryOptions.BuildQuery(ref query);

                entities = await query.ToArrayAsync(cToken);
            }

            if (!entities.Any())
                return entities;

            foreach (var entity in entities)
                options.Update(entity);

            await SaveChangesAsync(cToken);

            if (!IsExternalTransaction && Database.CurrentTransaction is not null)
                await Database.CurrentTransaction.CommitAsync(cToken);

            return entities;
        }
        catch
        {
            if (Database.CurrentTransaction is not null)
                await Database.CurrentTransaction.RollbackAsync(cToken);

            throw;
        }
        finally
        {
            if (!IsExternalTransaction && Database.CurrentTransaction is not null)
                await Database.CurrentTransaction.DisposeAsync();
        }
    }

    public async Task<long> Delete<T>(PersistenceQueryOptions<T> options, CancellationToken cToken) where T : class, IPersistent, IPersistentSql
    {
        try
        {
            if (!IsExternalTransaction && Database.CurrentTransaction is null)
                await Database.BeginTransactionAsync(cToken);

            var query = GetQuery<T>();

            options.BuildQuery(ref query);

            var entities = await query.ToArrayAsync(cToken);

            if (!entities.Any())
                return 0;

            await DeleteMany(entities, cToken);

            if (!IsExternalTransaction && Database.CurrentTransaction is not null)
                await Database.CurrentTransaction.CommitAsync(cToken);

            return entities.Length;
        }
        catch
        {
            if (Database.CurrentTransaction is not null)
                await Database.CurrentTransaction.RollbackAsync(cToken);

            throw;
        }
        finally
        {
            if (!IsExternalTransaction && Database.CurrentTransaction is not null)
                await Database.CurrentTransaction.DisposeAsync();
        }
    }

    public Task StartTransaction(CancellationToken cToken)
    {
        if (Database.CurrentTransaction is not null)
        {
            return Task.CompletedTask;
        }
        else
        {
            IsExternalTransaction = true;
            return Database.BeginTransactionAsync(cToken);
        }
    }
    public async Task CommitTransaction(CancellationToken cToken)
    {
        if (Database.CurrentTransaction is null)
            throw new InvalidOperationException("No transaction to commit.");

        try
        {
            await Database.CurrentTransaction.CommitAsync(cToken);
        }
        catch
        {
            throw;
        }
        finally
        {
            IsExternalTransaction = false;

            if (Database.CurrentTransaction is not null)
                await Database.CurrentTransaction.DisposeAsync();
        }
    }
    public async Task RollbackTransaction(CancellationToken cToken)
    {
        if (Database.CurrentTransaction is null)
            return;

        try
        {
            await Database.CurrentTransaction.RollbackAsync(cToken);
        }
        catch
        {
            throw;
        }
        finally
        {
            IsExternalTransaction = false;

            if (Database.CurrentTransaction is not null)
                await Database.CurrentTransaction.DisposeAsync();
        }
    }
}
