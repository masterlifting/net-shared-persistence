using System.Linq.Expressions;

using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
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
    private bool _isExternalTransaction;
    private IMongoCollection<T> GetCollection<T>() where T : class, IPersistentNoSql => _dataBase.GetCollection<T>(typeof(T).Name);
    private IMongoQueryable<T> SetCollection<T>() where T : class, IPersistentNoSql => GetCollection<T>().AsQueryable();

    protected MongoDbContext(MongoDbConnection connectionSettings)
    {
        _client = new MongoClient(connectionSettings.ConnectionString);
        _dataBase = _client.GetDatabase(connectionSettings.Database);
        OnModelCreating(new MongoDbBuilder(_dataBase));
    }

    public virtual void OnModelCreating(MongoDbBuilder builder)
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

    public async Task CreateOne<T>(T entity, CancellationToken cToken = default) where T : class, IPersistentNoSql
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

    public async Task<T[]> Update<T>(Expression<Func<T, bool>> filter, Action<T> updater, CancellationToken cToken) where T : class, IPersistentNoSql
    {
        try
        {
            if (!_isExternalTransaction && _session is null)
            {
                _session = await _client.StartSessionAsync(null, cToken);
                _session.StartTransaction();
            }

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
    public async Task<T[]> Delete<T>(Expression<Func<T, bool>> filter, CancellationToken cToken = default) where T : class, IPersistentNoSql
    {
        try
        {
            if (!_isExternalTransaction && _session is null)
            {
                _session = await _client.StartSessionAsync(null, cToken);
                _session.StartTransaction();
            }

            var collection = GetCollection<T>();

            var documents = collection.AsQueryable().Where(filter).ToArray();

            if (!documents.Any())
                return documents;

            await collection.DeleteManyAsync(_session, filter, null, cToken);

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

    public async Task StartTransaction(CancellationToken cToken = default)
    {
        if (_session?.IsInTransaction is true)
            return;

        _session = await _client.StartSessionAsync(null, cToken);
        _session.StartTransaction();
        _isExternalTransaction = true;
    }
    public async Task CommitTransaction(CancellationToken cToken = default)
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
    public async Task RollbackTransaction(CancellationToken cToken = default)
    {
        if (_session?.IsInTransaction != true)
            throw new PersistenceException("The transaction session was not found");
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
