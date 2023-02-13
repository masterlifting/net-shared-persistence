using System.Linq.Expressions;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Net.Shared.Persistence.Abstractions.Contexts;
using Net.Shared.Persistence.Abstractions.Entities;
using Net.Shared.Persistence.Settings.Connections;

using SharpCompress.Common;

namespace Net.Shared.Persistence.Contexts;

public abstract class PostgreContext : DbContext, IPersistencePostgreContext
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly PostgreSQLConnectionSettings _connectionSettings;
    protected PostgreContext(ILoggerFactory loggerFactory, PostgreSQLConnectionSettings connectionSettings)
    {
        _loggerFactory = loggerFactory;
        _connectionSettings = connectionSettings;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder builder)
    {
        builder.UseLoggerFactory(_loggerFactory);
        builder.UseNpgsql(_connectionSettings.GetConnectionString());
        base.OnConfiguring(builder);
    }

    public new IQueryable<T> Set<T>() where T : class, IPersistentSql => base.Set<T>();

    public Task<T?> FindByIdAsync<T>(CancellationToken cToken, object[] id) where T : class, IPersistentSql =>
        base.Set<T>().FindAsync(id, cToken).AsTask();
    public Task<T[]> FindManyAsync<T>(Expression<Func<T, bool>> condition, CancellationToken cToken = default) where T : class, IPersistentSql =>
        Set<T>().Where(condition).ToArrayAsync(cToken);
    public Task<T?> FindFirstAsync<T>(Expression<Func<T, bool>> condition, CancellationToken cToken = default) where T : class, IPersistentSql =>
        Set<T>().FirstOrDefaultAsync(condition, cToken);
    public Task<T?> FindSingleAsync<T>(Expression<Func<T, bool>> condition, CancellationToken cToken = default) where T : class, IPersistentSql =>
        Set<T>().SingleOrDefaultAsync(condition, cToken);

    public async Task CreateAsync<T>(T entity, CancellationToken cToken = default) where T : class, IPersistentSql
    {
        await base.Set<T>().AddAsync(entity, cToken);
        await SaveChangesAsync();
    }
    public async Task CreateManyAsync<T>(IReadOnlyCollection<T> entities, CancellationToken cToken = default) where T : class, IPersistentSql
    {
        await base.Set<T>().AddRangeAsync(entities, cToken);
        await SaveChangesAsync();
    }
    public async Task<T[]> UpdateAsync<T>(Expression<Func<T, bool>> condition, T entity, CancellationToken cToken = default) where T : class, IPersistentSql
    {
        var entities = await FindManyAsync(condition, cToken);

        if (!entities.Any())
            return entities;

        var entityProperties = typeof(T).GetProperties();
        var entityPropertiesDictionary = entityProperties.ToDictionary(x => x.Name, x => x.GetValue(entity));

        for (int i = 0; i < entities.Length; i++)
        {
            for (int j = 0; j < entityProperties.Length; j++)
            {
                var newValue = entityPropertiesDictionary[entityProperties[j].Name];

                if (newValue == default)
                    continue;

                var oldValue = entityProperties[j].GetValue(entities[i]);

                if (oldValue != newValue)
                    entityProperties[j].SetValue(entities[i], newValue);
            }
        }

        if (entities.Length == 1)
            base.Set<T>().Update(entities[0]);
        else
            base.Set<T>().UpdateRange(entities);

        await SaveChangesAsync(cToken);

        return entities;
    }
    public async Task<T[]> DeleteAsync<T>(Expression<Func<T, bool>> condition, CancellationToken cToken = default) where T : class, IPersistentSql
    {
        var entities = await FindManyAsync(condition, cToken);

        if (!entities.Any())
            return entities;

        base.Set<T>().RemoveRange(entities);

        await SaveChangesAsync(cToken);

        return entities;
    }

    public Task StartTransactionAsync(CancellationToken cToken = default) => Database.BeginTransactionAsync(cToken);
    public Task CommitTransactionAsync(CancellationToken cToken = default) => Database.CommitTransactionAsync(cToken);
    public Task RollbackTransactionAsync(CancellationToken cToken = default) => Database.RollbackTransactionAsync(cToken);
}
