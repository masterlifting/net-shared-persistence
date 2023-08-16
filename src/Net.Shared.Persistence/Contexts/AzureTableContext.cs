using Azure.Data.Tables;

using Net.Shared.Persistence.Abstractions.Contexts;
using Net.Shared.Persistence.Abstractions.Entities;
using Net.Shared.Persistence.Models.Contexts;

namespace Net.Shared.Persistence.Contexts;

public abstract class AzureTableContext : IPersistenceContext<ITableEntity>
{
    readonly TableServiceClient _tableServiceClient;
    public AzureTableContext(string connectionString)
    {
        _tableServiceClient = new TableServiceClient(connectionString);

        OnModelCreating(new AzureTableBuilder(_tableServiceClient));
    }
    public virtual void OnModelCreating(AzureTableBuilder azureTableBuilder)
    {
    }

    public Task CreateMany<T>(IReadOnlyCollection<T> entities, CancellationToken cToken) where T : class, IPersistent, ITableEntity
    {
        throw new NotImplementedException();
    }
    public Task CreateOne<T>(T entity, CancellationToken cToken) where T : class, IPersistent, ITableEntity
    {
        throw new NotImplementedException();
    }
    public Task<long> Delete<T>(PersistenceQueryOptions<T> options, CancellationToken cToken) where T : class, IPersistent, ITableEntity
    {
        throw new NotImplementedException();
    }
    public Task<T?> FindFirst<T>(PersistenceQueryOptions<T> options, CancellationToken cToken) where T : class, IPersistent, ITableEntity
    {
        throw new NotImplementedException();
    }
    public Task<T[]> FindMany<T>(PersistenceQueryOptions<T> options, CancellationToken cToken) where T : class, IPersistent, ITableEntity
    {
        throw new NotImplementedException();
    }
    public Task<TResult[]> FindMany<T, TResult>(PersistenceSelectorOptions<T, TResult> options, CancellationToken cToken) where T : class, IPersistent, ITableEntity
    {
        throw new NotImplementedException();
    }
    public Task<T?> FindSingle<T>(PersistenceQueryOptions<T> options, CancellationToken cToken) where T : class, IPersistent, ITableEntity
    {
        throw new NotImplementedException();
    }
    public IQueryable<T> GetQuery<T>() where T : class, IPersistent, ITableEntity
    {
        throw new NotImplementedException();
    }
    public Task<bool> IsExists<T>(PersistenceQueryOptions<T> options, CancellationToken cToken) where T : class, IPersistent, ITableEntity
    {
        throw new NotImplementedException();
    }
    public async Task<T[]> Update<T>(PersistenceUpdateOptions<T> options, CancellationToken cToken) where T : class, IPersistent, ITableEntity
    {
        var tableName = typeof(T).Name;
        var tableClient = _tableServiceClient.GetTableClient(tableName);
        var query = tableClient.Query<T>().AsQueryable();
        options.QueryOptions.BuildQuery(ref query);
        var results = query.ToArray();
        return results;
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

    public void SetTable<T>() where T : class, IPersistent, ITableEntity
    {
        var tableName = typeof(T).Name;

        _ = _serviceClient.CreateTableIfNotExists(tableName);
    }
    public void SetTable<T>(IEnumerable<T> entities) where T : class, IPersistent, ITableEntity
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
