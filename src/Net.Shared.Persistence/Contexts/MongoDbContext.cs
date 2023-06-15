using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

using Net.Shared.Persistence.Abstractions.Contexts;
using Net.Shared.Persistence.Abstractions.Entities;
using Net.Shared.Persistence.Models.Contexts;
using Net.Shared.Persistence.Models.Exceptions;
using Net.Shared.Persistence.Models.Settings.Connections;

namespace Net.Shared.Persistence.Contexts;

public abstract class MongoDbContext : IPersistenceNoSqlContext
{
    private readonly IMongoDatabase _dataBase;
    private readonly MongoClient _client;
    private IClientSessionHandle? _session;
    private bool _isExternalTransaction;

    private IMongoCollection<T> GetCollection<T>() where T : class, IPersistentNoSql => _dataBase.GetCollection<T>(typeof(T).Name);
    public IQueryable<T> GetQuery<T>() where T : class, IPersistentNoSql => GetCollection<T>().AsQueryable();

    protected MongoDbContext(MongoDbConnection connectionSettings)
    {
        _client = new MongoClient(connectionSettings.ConnectionString);
        _dataBase = _client.GetDatabase(connectionSettings.Database);

        OnModelCreating(new MongoDbBuilder(_dataBase));
    }

    public virtual void OnModelCreating(MongoDbBuilder builder)
    {
    }

    public Task<bool> IsExists<T>(PersistenceQueryOptions<T> options, CancellationToken cToken) where T : class, IPersistentNoSql
    {
        var query = GetQuery<T>();
        options.BuildQuery(ref query);
        return Task.Run(() => query.Any(), cToken);
    }

    public Task<T?> FindFirst<T>(PersistenceQueryOptions<T> options, CancellationToken cToken) where T : class, IPersistentNoSql
    {
        var query = GetQuery<T>();
        options.BuildQuery(ref query);
        return Task.Run(() => query.FirstOrDefault(), cToken);
    }
    public Task<T?> FindSingle<T>(PersistenceQueryOptions<T> options, CancellationToken cToken) where T : class, IPersistentNoSql
    {
        var query = GetQuery<T>();
        options.BuildQuery(ref query);
        return Task.Run(() => query.SingleOrDefault(), cToken);
    }

    public Task<T[]> FindMany<T>(PersistenceQueryOptions<T> options, CancellationToken cToken) where T : class, IPersistentNoSql
    {
        var query = GetQuery<T>();
        options.BuildQuery(ref query);
        return Task.Run(() => query.ToArray(), cToken);
    }
    public Task<TResult[]> FindMany<T, TResult>(PersistenceSelectorOptions<T, TResult> options, CancellationToken cToken) where T : class, IPersistentNoSql
    {
        var query = GetQuery<T>();
        options.QueryOptions.BuildQuery(ref query);
        return Task.Run(() => query.Select(options.Selector).ToArray(), cToken);
    }

    public async Task CreateOne<T>(T entity, CancellationToken cToken) where T : class, IPersistentNoSql
    {
        try
        {
            await GetCollection<T>().InsertOneAsync(entity, null, cToken);
            return;
        }
        catch (MongoException exception)
        {
            throw new PersistenceException(exception);
        }
    }
    public async Task CreateMany<T>(IReadOnlyCollection<T> entities, CancellationToken cToken) where T : class, IPersistentNoSql
    {
        try
        {
            await GetCollection<T>().InsertManyAsync(entities, null, cToken);
            return;
        }
        catch (MongoException exception)
        {
            throw new PersistenceException(exception);
        }
    }

    public async Task<T[]> Update<T>(PersistenceQueryOptions<T> options, Action<T> updater, CancellationToken cToken) where T : class, IPersistentNoSql
    {
        try
        {
            if (!_isExternalTransaction && _session is null)
            {
                _session = await _client.StartSessionAsync(null, cToken);
                _session.StartTransaction();
            }

            var collection = GetCollection<T>();

            IQueryable<T> query = collection.AsQueryable();

            options.BuildQuery(ref query);

            var documents = query.ToArray();

            if (!documents.Any())
                return documents;

            foreach (var document in documents)
            {
                updater(document);
            }

            await collection.DeleteManyAsync<T>(options.Filter, cToken);
            await collection.InsertManyAsync(_session, documents, null, cToken);

            if (!_isExternalTransaction && _session?.IsInTransaction is true)
                await _session.CommitTransactionAsync(cToken);

            return documents;
        }
        catch (Exception exception)
        {
            if (!_isExternalTransaction && _session?.IsInTransaction is true)
                await _session.AbortTransactionAsync(cToken);

            throw new PersistenceException(exception);
        }
        finally
        {
            if (!_isExternalTransaction)
                Dispose();
        }
    }
    public async Task Update<T>(PersistenceQueryOptions<T> options, IEnumerable<T> data, CancellationToken cToken) where T : class, IPersistentNoSql
    {
        try
        {
            if (!_isExternalTransaction && _session is null)
            {
                _session = await _client.StartSessionAsync(null, cToken);
                _session.StartTransaction();
            }

            var collection = GetCollection<T>();

            await collection.DeleteManyAsync<T>(options.Filter, cToken);
            await collection.InsertManyAsync(_session, data, null, cToken);

            if (!_isExternalTransaction && _session?.IsInTransaction is true)
                await _session.CommitTransactionAsync(cToken);
        }
        catch (Exception exception)
        {
            if (!_isExternalTransaction && _session?.IsInTransaction is true)
                await _session.AbortTransactionAsync(cToken);

            throw new PersistenceException(exception);
        }
        finally
        {
            if (!_isExternalTransaction)
                Dispose();
        }
    }

    public async Task<T[]> Delete<T>(PersistenceQueryOptions<T> options, CancellationToken cToken) where T : class, IPersistentNoSql
    {
        try
        {
            if (!_isExternalTransaction && _session is null)
            {
                _session = await _client.StartSessionAsync(null, cToken);
                _session.StartTransaction();
            }

            var collection = GetCollection<T>();

            IQueryable<T> query = collection.AsQueryable();

            options.BuildQuery(ref query);

            var documents = query.ToArray();

            if (!documents.Any())
                return documents;

            await collection.DeleteManyAsync(_session, options.Filter, null, cToken);

            if (!_isExternalTransaction && _session?.IsInTransaction is true)
                await _session.CommitTransactionAsync(cToken);

            return documents;
        }
        catch (Exception exception)
        {
            if (!_isExternalTransaction && _session?.IsInTransaction is true)
                await _session.AbortTransactionAsync(cToken);

            throw new PersistenceException(exception);
        }
        finally
        {
            if (!_isExternalTransaction)
                Dispose();
        }
    }

    public async Task StartTransaction(CancellationToken cToken)
    {
        if (_session?.IsInTransaction is true)
            return;

        _session = await _client.StartSessionAsync(null, cToken);
        _session.StartTransaction();
        _isExternalTransaction = true;
    }
    public async Task CommitTransaction(CancellationToken cToken)
    {
        if (_session?.IsInTransaction != true)
            throw new PersistenceException("The transaction session was not found");

        try
        {
            await _session.CommitTransactionAsync(cToken);
        }
        catch (Exception exception)
        {
            throw new PersistenceException(exception);
        }
        finally
        {
            _isExternalTransaction = false;
            Dispose();
        }
    }
    public async Task RollbackTransaction(CancellationToken cToken)
    {
        if (_session?.IsInTransaction != true)
            return;

        try
        {
            await _session.AbortTransactionAsync(cToken);
        }
        catch (Exception exception)
        {
            throw new PersistenceException(exception);
        }
        finally
        {
            _isExternalTransaction = false;
            Dispose();
        }
    }

    public void Dispose()
    {
        _session?.Dispose();
        _session = null;
    }
}
public sealed class MongoDbBuilder
{
    private readonly IMongoDatabase _database;
    public MongoDbBuilder(IMongoDatabase database) => _database = database;

    public IMongoCollection<T> SetCollection<T>(CreateCollectionOptions? options = null) where T : class, IPersistentNoSql
    {
        var collectionName = typeof(T).Name;

        if (!_database.ListCollectionNames().ToList().Contains(collectionName))
        {
            var guidSerializer = BsonSerializer.LookupSerializer<Guid>();

            if (guidSerializer == null)
            {
                BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
            }

            _database.CreateCollection(collectionName, options);

            return _database.GetCollection<T>(collectionName);
        }

        return _database.GetCollection<T>(collectionName);
    }
    public IMongoCollection<T> SetCollection<T>(IEnumerable<T> items, CreateCollectionOptions? options = null) where T : class, IPersistentNoSql
    {
        var collection = SetCollection<T>(options);

        if (items.Any() && collection.CountDocuments(new BsonDocument()) == 0)
            collection.InsertMany(items);

        return collection;
    }
}
