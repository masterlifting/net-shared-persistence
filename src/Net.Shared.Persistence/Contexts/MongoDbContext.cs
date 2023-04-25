using System.Linq.Expressions;

using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

using Net.Shared.Persistence.Abstractions.Contexts;
using Net.Shared.Persistence.Abstractions.Entities;
using Net.Shared.Persistence.Models.Exceptions;
using Net.Shared.Persistence.Models.Settings.Connections;

namespace Net.Shared.Persistence.Contexts;

public abstract class MongoDbContext : IPersistenceNoSqlContext
{
    private readonly IMongoDatabase _dataBase;
    private readonly MongoClient _client;
    private IClientSessionHandle? _session;
    private IMongoCollection<T> GetCollection<T>() where T : class, IPersistentNoSql => _dataBase.GetCollection<T>(typeof(T).Name);
    private IMongoQueryable<T> SetCollection<T>() where T : class, IPersistentNoSql => GetCollection<T>().AsQueryable();

    protected MongoDbContext(MongoDbConnection connectionSettings)
    {
        _client = new MongoClient(connectionSettings.ConnectionString);
        _dataBase = _client.GetDatabase(connectionSettings.Database);
        OnModelCreating(new MongoModelBuilder(_dataBase));
    }

    public virtual void OnModelCreating(MongoModelBuilder builder)
    {
    }

    public IQueryable<T> SetEntity<T>() where T : class, IPersistentNoSql => SetCollection<T>();

    public Task<T[]> FindAll<T>(CancellationToken cToken) where T : class, IPersistentNoSql =>
        Task.Run(() => SetEntity<T>().ToArray(), cToken);
    public Task<T[]> FindMany<T>(Expression<Func<T, bool>> filter, CancellationToken cToken = default) where T : class, IPersistentNoSql =>
            Task.Run(() => SetEntity<T>().Where(filter).ToArray(), cToken);
    public Task<T?> FindFirst<T>(Expression<Func<T, bool>> filter, CancellationToken cToken = default) where T : class, IPersistentNoSql =>
        Task.Run(() => SetEntity<T>().FirstOrDefault(filter), cToken);
    public Task<T?> FindSingle<T>(Expression<Func<T, bool>> filter, CancellationToken cToken = default) where T : class, IPersistentNoSql =>
        Task.Run(() => SetEntity<T>().SingleOrDefault(filter), cToken);

    public Task CreateOne<T>(T entity, CancellationToken cToken = default) where T : class, IPersistentNoSql =>
        GetCollection<T>().InsertOneAsync(entity, null, cToken);
    public Task CreateMany<T>(IReadOnlyCollection<T> entities, CancellationToken cToken) where T : class, IPersistentNoSql =>
        GetCollection<T>().InsertManyAsync(entities, null, cToken);

    public async Task<T[]> Update<T>(Expression<Func<T, bool>> filter, Action<T> updater, CancellationToken cToken) where T : class, IPersistentNoSql
    {
        try
        {
            await StartTransaction(cToken);

            var collection = GetCollection<T>();

            var documents = collection.AsQueryable().Where(filter).ToArray();

            if (!documents.Any())
                return documents;

            foreach (var document in documents)
            {
                updater(document);

                var replaceOptions = new ReplaceOptions 
                { 
                    IsUpsert = false 
                };

                var result = await collection.ReplaceOneAsync(_session, filter, document, replaceOptions, cToken);
            }

            await CommitTransaction(cToken);

            return documents;
        }
        catch
        {
            await RollbackTransaction(cToken);
            throw;
        }
    }
    public async Task<T[]> Delete<T>(Expression<Func<T, bool>> filter, CancellationToken cToken = default) where T : class, IPersistentNoSql
    {
       try
        {
            await StartTransaction(cToken);

            var collection = GetCollection<T>();
            
            var documents = collection.AsQueryable().Where(filter).ToArray();

            if (!documents.Any())
                return documents;
            
            await collection.DeleteManyAsync(_session, filter, null, cToken);
            
            await CommitTransaction(cToken);
            
            return documents;
        }
        catch
        {
            await RollbackTransaction(cToken);
            throw;
        }
    }

    public async Task StartTransaction(CancellationToken cToken = default)
    {
        if (_session is not null)
            return;

        _session = await _client.StartSessionAsync(null, cToken);
        _session.StartTransaction();
    }
    public async Task CommitTransaction(CancellationToken cToken = default)
    {
        if (_session is null)
            throw new PersistenceException("The transaction session was not found");

        try
        {
            await _session.CommitTransactionAsync(cToken);
        }
        catch
        {
            throw;
        }
        finally
        {
            _session.Dispose();
        }
    }
    public async Task RollbackTransaction(CancellationToken cToken = default)
    {
        if (_session is null)
            throw new PersistenceException("The transaction session was not found");
        try
        {
            await _session.CommitTransactionAsync(cToken);
        }
        catch
        {
            throw;
        }
        finally
        {
            _session.Dispose();
        }
    }

    public void Dispose() => _session?.Dispose();
}
public sealed class MongoModelBuilder
{
    private readonly IMongoDatabase _database;
    public MongoModelBuilder(IMongoDatabase database) => _database = database;

    public IMongoCollection<T> SetCollection<T>(CreateCollectionOptions? options = null) where T : class, IPersistentNoSql
    {
        var collection = _database.GetCollection<T>(typeof(T).Name);

        if (collection is null)
        {
            _database.CreateCollection(typeof(T).Name, options);
            collection = _database.GetCollection<T>(typeof(T).Name);
        }

        return collection;
    }
    public IMongoCollection<T> SetCollection<T>(IEnumerable<T> items, CreateCollectionOptions? options = null) where T : class, IPersistentNoSql
    {
        var collection = SetCollection<T>(options);

        if (items.Any() && collection.CountDocuments(new BsonDocument()) == 0)
            collection.InsertMany(items);

        return collection;
    }
}
