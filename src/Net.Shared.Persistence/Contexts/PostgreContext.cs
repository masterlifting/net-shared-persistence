using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Net.Shared.Persistence.Abstractions.Contexts;
using Net.Shared.Persistence.Abstractions.Entities;
using Net.Shared.Persistence.Models.Exceptions;
using Net.Shared.Persistence.Models.Settings.Connections;

using System.Linq.Expressions;

namespace Net.Shared.Persistence.Contexts;

public abstract class PostgreContext : DbContext, IPersistencePostgreContext
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly PostgreConnectionSettings _connectionSettings;


    protected PostgreContext(ILoggerFactory loggerFactory, PostgreConnectionSettings connectionSettings)
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

    public string GetTableName<T>() where T : class, IPersistentSql => 
        Model.FindEntityType(typeof(T))?.ShortName() ?? throw new NetSharedPersistenceException($"Searching a table name {typeof(T).Name} was not found.");

    public IQueryable<T> FromSqlRaw<T>(string sqlQuery) where T : class, IPersistentSql => 
        base.Set<T>().FromSqlRaw(sqlQuery);

    public Task<T?> FindById<T>(object[] id, CancellationToken cToken) where T : class, IPersistentSql =>
        base.Set<T>().FindAsync(id, cToken).AsTask();
    public Task<T[]> FindMany<T>(Expression<Func<T, bool>> condition, CancellationToken cToken = default) where T : class, IPersistentSql =>
        Set<T>().Where(condition).ToArrayAsync(cToken);
    public Task<T?> FindFirst<T>(Expression<Func<T, bool>> condition, CancellationToken cToken = default) where T : class, IPersistentSql =>
        Set<T>().FirstOrDefaultAsync(condition, cToken);
    public Task<T?> FindSingle<T>(Expression<Func<T, bool>> condition, CancellationToken cToken = default) where T : class, IPersistentSql =>
        Set<T>().SingleOrDefaultAsync(condition, cToken);

    public async Task CreateOne<T>(T entity, CancellationToken cToken = default) where T : class, IPersistentSql
    {
        await base.Set<T>().AddAsync(entity, cToken);
        await SaveChangesAsync(cToken);
    }
    public async Task CreateMany<T>(IReadOnlyCollection<T> entities, CancellationToken cToken = default) where T : class, IPersistentSql
    {
        await base.Set<T>().AddRangeAsync(entities, cToken);
        await SaveChangesAsync(cToken);
    }
    public async Task<T[]> Update<T>(Expression<Func<T, bool>> condition, T entity, CancellationToken cToken = default) where T : class, IPersistentSql
    {
        var entities = await FindMany(condition, cToken);

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
    public async Task<T[]> Delete<T>(Expression<Func<T, bool>> condition, CancellationToken cToken = default) where T : class, IPersistentSql
    {
        var entities = await FindMany(condition, cToken);

        if (!entities.Any())
            return entities;

        base.Set<T>().RemoveRange(entities);

        await SaveChangesAsync(cToken);

        return entities;
    }

    public Task StartTransaction(CancellationToken cToken = default) => Database.BeginTransactionAsync(cToken);
    public Task CommitTransaction(CancellationToken cToken = default) => Database.CommitTransactionAsync(cToken);
    public Task RollbackTransaction(CancellationToken cToken = default) => Database.RollbackTransactionAsync(cToken);
}
