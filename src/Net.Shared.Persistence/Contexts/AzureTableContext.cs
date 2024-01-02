using Azure.Data.Tables;

using Net.Shared.Persistence.Abstractions.Interfaces.Contexts;
using Net.Shared.Persistence.Abstractions.Interfaces.Entities;
using Net.Shared.Persistence.Abstractions.Models.Contexts;
using Net.Shared.Persistence.Abstractions.Models.Settings.Connections;

namespace Net.Shared.Persistence.Contexts;

public abstract class AzureTableContext : IPersistenceContext<ITableEntity>
{
    readonly TableServiceClient _tableServiceClient;

    private TableClient GetTableClient<T>() where T : class, IPersistent, ITableEntity => _tableServiceClient.GetTableClient(typeof(T).Name);
    private static IQueryable<T> GetQuery<T>(TableClient client) where T : class, IPersistent, ITableEntity =>
        client.Query<T>().AsQueryable();
    public IQueryable<T> GetQuery<T>() where T : class, IPersistent, ITableEntity =>
        GetTableClient<T>().Query<T>().AsQueryable();

    public AzureTableContext(AzureTableConnectionSettings connectionSettings)
    {
        _tableServiceClient = new TableServiceClient(connectionSettings.ConnectionString);

        OnModelCreating(new AzureTableBuilder(_tableServiceClient));
    }
    public abstract void OnModelCreating(AzureTableBuilder azureTableBuilder);


    public Task<bool> IsExists<T>(PersistenceQueryOptions<T> options, CancellationToken cToken) where T : class, IPersistent, ITableEntity
    {
        var query = GetQuery<T>();
        options.BuildQuery(ref query);
        return Task.FromResult(query.Count() > 0);
    }

    public Task<T?> FindFirst<T>(PersistenceQueryOptions<T> options, CancellationToken cToken) where T : class, IPersistent, ITableEntity
    {
        throw new NotImplementedException();
    }
    public Task<T?> FindSingle<T>(PersistenceQueryOptions<T> options, CancellationToken cToken) where T : class, IPersistent, ITableEntity
    {
        var query = GetQuery<T>();
        options.BuildQuery(ref query);
        return Task.FromResult(query.SingleOrDefault());
    }

    public Task<T[]> FindMany<T>(PersistenceQueryOptions<T> options, CancellationToken cToken) where T : class, IPersistent, ITableEntity
    {
        var query = GetQuery<T>();
        options.BuildQuery(ref query);
        return Task.FromResult(query.ToArray());
    }
    public Task<TResult[]> FindMany<T, TResult>(PersistenceSelectorOptions<T, TResult> options, CancellationToken cToken) where T : class, IPersistent, ITableEntity
    {
        throw new NotImplementedException();
    }

    public Task CreateOne<T>(T entity, CancellationToken cToken) where T : class, IPersistent, ITableEntity
    {
        var tableClient = GetTableClient<T>();
        return tableClient.AddEntityAsync(entity, cToken);
    }
    public Task CreateMany<T>(IReadOnlyCollection<T> entities, CancellationToken cToken) where T : class, IPersistent, ITableEntity
    {
        throw new NotImplementedException();
    }

    public async Task<T[]> Update<T>(PersistenceUpdateOptions<T> options, CancellationToken cToken) where T : class, IPersistent, ITableEntity
    {
        var client = GetTableClient<T>();

        T[] rows;

        if (options.Data is not null)
        {
            rows = options.Data;
        }
        else
        {
            var query = GetQuery<T>(client);

            options.QueryOptions.BuildQuery(ref query);

            rows = query.ToArray();
        }

        if (!rows.Any())
            return rows;

        foreach (var row in rows)
        {
            _ = options.Update(row);

            var result = await client.UpdateEntityAsync(row, row.ETag, TableUpdateMode.Merge, cToken);

            row.ETag = result.Headers.ETag!.Value;
        }

        return rows;
    }

    public Task<long> Delete<T>(PersistenceQueryOptions<T> options, CancellationToken cToken) where T : class, IPersistent, ITableEntity
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

        if (response.Value is not null)
        {
            var tableClient = _serviceClient.GetTableClient(tableName);

            foreach (var entity in entities)
            {
                tableClient.AddEntity(entity);
            }
        }
    }
}
