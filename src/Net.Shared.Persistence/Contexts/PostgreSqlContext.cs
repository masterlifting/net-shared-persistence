using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Net.Shared.Persistence.Abstractions.Contexts;
using Net.Shared.Persistence.Abstractions.Entities;
using Net.Shared.Persistence.Models.Exceptions;
using Net.Shared.Persistence.Models.Settings.Connections;

namespace Net.Shared.Persistence.Contexts;

public abstract class PostgreSqlContext : DbContext, IPersistenceSqlContext
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly PostgreSqlConnection _connectionSettings;

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
    public IQueryable<T> SetEntity<T>() where T : class, IPersistentSql => base.Set<T>();

    public string GetTableName<T>() where T : class, IPersistentSql =>
        Model.FindEntityType(typeof(T))?.ShortName() ?? throw new PersistenceException($"Searching a table name {typeof(T).Name} was not found.");

    public IQueryable<T> GetQueryFromRaw<T>(FormattableString query, CancellationToken cToken = default) where T : class, IPersistentSql =>
        base.Set<T>().FromSqlRaw(query.Format);

    public Task<T?> FindById<T>(object[] id, CancellationToken cToken) where T : class, IPersistentSql =>
        base.Set<T>().FindAsync(id, cToken).AsTask();

    public Task<T[]> FindAll<T>(CancellationToken cToken) where T : class, IPersistentSql => SetEntity<T>().ToArrayAsync(cToken);
    public Task<T[]> FindMany<T>(Expression<Func<T, bool>> filter, CancellationToken cToken = default) where T : class, IPersistentSql =>
        SetEntity<T>().Where(filter).ToArrayAsync(cToken);
    public Task<T?> FindFirst<T>(Expression<Func<T, bool>> filter, CancellationToken cToken = default) where T : class, IPersistentSql =>
        SetEntity<T>().FirstOrDefaultAsync(filter, cToken);
    public Task<T?> FindSingle<T>(Expression<Func<T, bool>> filter, CancellationToken cToken = default) where T : class, IPersistentSql =>
        SetEntity<T>().SingleOrDefaultAsync(filter, cToken);

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

    public Task<T[]> Update<T>(Expression<Func<T, bool>> filter, T entity, CancellationToken cToken) where T : class, IPersistentSql
    {
        throw new NotImplementedException();
    }

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

    public async Task<T[]> Delete<T>(Expression<Func<T, bool>> filter, CancellationToken cToken = default) where T : class, IPersistentSql
    {
        var entities = await FindMany(filter, cToken);

        if (!entities.Any())
            return entities;

        base.Set<T>().RemoveRange(entities);

        await SaveChangesAsync(cToken);

        return entities;
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

    public Task StartTransaction(CancellationToken cToken = default) => Database.BeginTransactionAsync(cToken);
    public Task CommitTransaction(CancellationToken cToken = default) => Database.CommitTransactionAsync(cToken);
    public Task RollbackTransaction(CancellationToken cToken = default) => Database.RollbackTransactionAsync(cToken);
}
