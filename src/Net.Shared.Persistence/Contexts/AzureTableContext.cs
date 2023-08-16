using Azure.Data.Tables;

using Net.Shared.Persistence.Abstractions.Contexts;
using Net.Shared.Persistence.Abstractions.Entities;
using Net.Shared.Persistence.Models.Contexts;

namespace Net.Shared.Persistence.Contexts;

public abstract class AzureTableContext : IPersistenceNoSqlContext
{
    readonly TableServiceClient _tableServiceClient;
    public AzureTableContext(string connectionString)
    {
        _tableServiceClient = new TableServiceClient(connectionString);

        OnModelCreating(new AzureTableBuilder(_tableServiceClient));
    }

    public virtual void OnModelCreating(AzureTableBuilder builder)
    {
    }

    public IQueryable<T> GetQuery<T>() where T : class, IPersistentNoSql
    {
        var tableClient = _tableServiceClient.GetTableClient(typeof(T).Name);
        return tableClient.Query<T>().AsQueryable();
    }

    public Task<bool> IsExists<T>(PersistenceQueryOptions<T> options, CancellationToken cToken) where T : class, IPersistentNoSql
    {
        throw new NotImplementedException();
    }

    public Task<T?> FindFirst<T>(PersistenceQueryOptions<T> options, CancellationToken cToken) where T : class, IPersistentNoSql
    {
        throw new NotImplementedException();
    }
    public Task<T?> FindSingle<T>(PersistenceQueryOptions<T> options, CancellationToken cToken) where T : class, IPersistentNoSql
    {
        throw new NotImplementedException();
    }

    public Task<T[]> FindMany<T>(PersistenceQueryOptions<T> options, CancellationToken cToken) where T : class, IPersistentNoSql
    {
        var query = GetQuery<T>();
        options.BuildQuery(ref query);
        return Task.Run(() => query.ToArray(), cToken);
    }
    public Task<TResult[]> FindMany<T, TResult>(PersistenceSelectorOptions<T, TResult> options, CancellationToken cToken) where T : class, IPersistentNoSql
    {
        throw new NotImplementedException();
    }

    public Task CreateOne<T>(T entity, CancellationToken cToken) where T : class, IPersistentNoSql
    {
        throw new NotImplementedException();
    }
    public Task CreateMany<T>(IReadOnlyCollection<T> entities, CancellationToken cToken) where T : class, IPersistentNoSql
    {
        throw new NotImplementedException();
    }

    public Task<T[]> Update<T>(PersistenceUpdateOptions<T> options, CancellationToken cToken) where T : class, IPersistentNoSql
    {
        var query = GetQuery<T>();
        throw new NotImplementedException();
    }

    public Task<long> Delete<T>(PersistenceQueryOptions<T> options, CancellationToken cToken) where T : class, IPersistentNoSql
    {
        throw new NotImplementedException();
    }

    public Task StartTransaction(CancellationToken cToken)
    {
        throw new NotImplementedException();
    }
    public Task CommitTransaction(CancellationToken cToken)
    {
        throw new NotImplementedException();
    }
    public Task RollbackTransaction(CancellationToken cToken)
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }
}

public sealed class AzureTableBuilder
{
    private readonly TableServiceClient _serviceClient;
    public AzureTableBuilder(TableServiceClient serviceClient) => _serviceClient = serviceClient;

    public void SetTable<T>() where T : class, IPersistentNoSql, ITableEntity
    {
        var tableName = typeof(T).Name;

        _ = _serviceClient.CreateTableIfNotExists(tableName);
    }
    public void SetTable<T>(IEnumerable<T> entities) where T : class, IPersistentNoSql, ITableEntity
    {
        var tableName = typeof(T).Name;

        var response = _serviceClient.CreateTableIfNotExists(tableName);
        
        if(response.Value is not null)
        {
            var tableClient = _serviceClient.GetTableClient(tableName);
            
            foreach (var entity in entities)
            {
                tableClient.AddEntity(entity);
            }
        }
    }
}
