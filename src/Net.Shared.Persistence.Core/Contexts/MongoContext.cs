using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

using Shared.Persistence.Abstractions.Contexts;
using Shared.Persistence.Abstractions.Entities;
using Shared.Persistence.Exceptions;
using Shared.Persistence.Settings.Connections;

using System.Linq.Expressions;

using static Shared.Persistence.Abstractions.Constants.Enums;

namespace Shared.Persistence.Contexts;

public abstract class MongoContext : IMongoPersistenceContext
{
    private readonly IMongoDatabase _dataBase;
    private readonly MongoClient _client;
    private IClientSessionHandle? _session;
    private IMongoCollection<T> GetCollection<T>() where T : class, IPersistentNoSql => _dataBase.GetCollection<T>(typeof(T).Name);
    private IMongoQueryable<T> SetCollection<T>() where T : class, IPersistentNoSql => GetCollection<T>().AsQueryable();

    protected MongoContext(MongoDBConnectionSettings connectionSettings)
    {
        var connectionString = connectionSettings.GetConnectionString();
        _client = new MongoClient(connectionString);
        _dataBase = _client.GetDatabase(connectionSettings.Database);
        OnModelCreating(new MongoModelBuilder(_dataBase));
    }

    public virtual void OnModelCreating(MongoModelBuilder builder)
    {
    }

    public IQueryable<T> Set<T>() where T : class, IPersistentNoSql => SetCollection<T>();

    public Task<T[]> FindManyAsync<T>(Expression<Func<T, bool>> condition, CancellationToken cToken = default) where T : class, IPersistentNoSql =>
            Task.Run(() => Set<T>().Where(condition).ToArray(), cToken);
    public Task<T?> FindFirstAsync<T>(Expression<Func<T, bool>> condition, CancellationToken cToken = default) where T : class, IPersistentNoSql =>
        Task.Run(() => Set<T>().FirstOrDefault(), cToken);
    public Task<T?> FindSingleAsync<T>(Expression<Func<T, bool>> condition, CancellationToken cToken = default) where T : class, IPersistentNoSql =>
        Task.Run(() => Set<T>().SingleOrDefault(), cToken);

    public Task CreateAsync<T>(T entity, CancellationToken cToken = default) where T : class, IPersistentNoSql =>
        GetCollection<T>().InsertOneAsync(entity, null, cToken);
    public Task CreateManyAsync<T>(IReadOnlyCollection<T> entities, CancellationToken cToken) where T : class, IPersistentNoSql =>
        GetCollection<T>().InsertManyAsync(entities, null, cToken);
    public async Task<T[]> UpdateAsync<T>(Expression<Func<T, bool>> condition, Dictionary<ContextCommand, (string Name, string Value)> updater, CancellationToken cToken = default) where T : class, IPersistentNoSql
    {
        var updateRules = new BsonDocument();

        foreach (var item in updater)
        {
            var field = item.Value;

            switch (item.Key)
            {
                case ContextCommand.Set:
                    {
                        updateRules.Add("$set", new BsonDocument(field.Name, field.Value));
                        break;
                    }
                case ContextCommand.Inc:
                    {
                        updateRules.Add("$inc", new BsonDocument(field.Name, field.Value));
                        break;
                    }
            }
        }

        var result = await GetCollection<T>().UpdateManyAsync<T>(condition, updateRules, null, cToken);


       return Array.Empty<T>();
    }
    public async Task<T[]> DeleteAsync<T>(Expression<Func<T, bool>> condition, CancellationToken cToken = default) where T : class, IPersistentNoSql
    {
        var collection = GetCollection<T>();

        var result = await collection.DeleteManyAsync<T>(condition, null, cToken);

        return Array.Empty<T>();   
    }

    public async Task StartTransactionAsync(CancellationToken cToken = default)
    {
        if (_session is not null)
            throw new SharedPersistenceException(nameof(MongoContext), nameof(StartTransactionAsync), new("The transaction session is already"));

        _session = await _client.StartSessionAsync();
        _session.StartTransaction();
    }
    public async Task CommitTransactionAsync(CancellationToken cToken = default)
    {
        if (_session is null)
            throw new SharedPersistenceException(nameof(MongoContext), nameof(CommitTransactionAsync), new("The transaction session was not found"));

        await _session.CommitTransactionAsync();
        _session.Dispose();
    }
    public async Task RollbackTransactionAsync(CancellationToken cToken = default)
    {
        if (_session is null)
            throw new SharedPersistenceException(nameof(MongoContext), nameof(RollbackTransactionAsync), new("The transaction session was not found"));

        await _session.CommitTransactionAsync();
        _session.Dispose();
    }

    public void Dispose() => _session?.Dispose();
}
public sealed class MongoModelBuilder
{
    private readonly IMongoDatabase _database;
    public MongoModelBuilder(IMongoDatabase database) => _database = database;

    public IMongoCollection<T> SetCollection<T>(IEnumerable<T>? items = null, CreateCollectionOptions? options = null) where T : class, IPersistentNoSql
    {
        var collection = _database.GetCollection<T>(typeof(T).Name);

        if (collection is null)
        {
            _database.CreateCollection(typeof(T).Name, options);
            collection = _database.GetCollection<T>(typeof(T).Name);
        }
        if (items is not null && items.Any() && collection.CountDocuments(new BsonDocument()) == 0)
            collection.InsertMany(items);

        return collection;
    }
}
