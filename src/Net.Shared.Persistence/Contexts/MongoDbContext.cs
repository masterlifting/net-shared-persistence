using System.Linq.Expressions;

using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

using Net.Shared.Persistence.Abstractions.Interfaces.Contexts;
using Net.Shared.Persistence.Abstractions.Interfaces.Entities;
using Net.Shared.Persistence.Abstractions.Models.Contexts;
using Net.Shared.Persistence.Abstractions.Models.Settings.Connections;

namespace Net.Shared.Persistence.Contexts;

public abstract class MongoDbContext : IPersistenceNoSqlContext
{
    private readonly IMongoDatabase _dataBase;
    private readonly MongoClient _client;
    private IClientSessionHandle? _session;
    private bool _isExternalTransaction;

    private IMongoCollection<T> GetCollection<T>() where T : class, IPersistent, IPersistentNoSql => _dataBase.GetCollection<T>(typeof(T).Name);
    public IQueryable<T> GetQuery<T>() where T : class, IPersistent, IPersistentNoSql => GetCollection<T>().AsQueryable();

    protected MongoDbContext(MongoDbConnectionSettings connectionSettings)
    {
        _client = new MongoClient(connectionSettings.ConnectionString);
        _dataBase = _client.GetDatabase(connectionSettings.Database);

        OnModelCreating(new MongoDbBuilder(_dataBase));
    }

    public abstract void OnModelCreating(MongoDbBuilder builder);

    public Task<bool> IsExists<T>(PersistenceQueryOptions<T> options, CancellationToken cToken) where T : class, IPersistent, IPersistentNoSql
    {
        var query = GetQuery<T>();
        options.BuildQuery(ref query);
        return Task.Run(() => query.Any(), cToken);
    }

    public Task<T?> FindFirst<T>(PersistenceQueryOptions<T> options, CancellationToken cToken) where T : class, IPersistent, IPersistentNoSql
    {
        var query = GetQuery<T>();
        options.BuildQuery(ref query);
        return Task.Run(() => query.FirstOrDefault(), cToken);
    }
    public Task<T?> FindSingle<T>(PersistenceQueryOptions<T> options, CancellationToken cToken) where T : class, IPersistent, IPersistentNoSql
    {
        var query = GetQuery<T>();
        options.BuildQuery(ref query);
        return Task.Run(() => query.SingleOrDefault(), cToken);
    }

    public Task<T[]> FindMany<T>(PersistenceQueryOptions<T> options, CancellationToken cToken) where T : class, IPersistent, IPersistentNoSql
    {
        var query = GetQuery<T>();
        options.BuildQuery(ref query);
        return Task.Run(() => query.ToArray(), cToken);
    }
    public Task<TResult[]> FindMany<T, TResult>(PersistenceSelectorOptions<T, TResult> options, CancellationToken cToken) where T : class, IPersistent, IPersistentNoSql
    {
        var query = GetQuery<T>();
        options.QueryOptions.BuildQuery(ref query);
        return Task.Run(() => query.Select(options.Selector).ToArray(), cToken);
    }

    public async Task CreateOne<T>(T entity, CancellationToken cToken) where T : class, IPersistent, IPersistentNoSql
    {
        await GetCollection<T>().InsertOneAsync(entity, null, cToken);
        return;
    }
    public async Task CreateMany<T>(IReadOnlyCollection<T> entities, CancellationToken cToken) where T : class, IPersistent, IPersistentNoSql
    {
        await GetCollection<T>().InsertManyAsync(entities, null, cToken);
        return;
    }

    public async Task<T[]> Update<T>(PersistenceUpdateOptions<T> options, CancellationToken cToken) where T : class, IPersistent, IPersistentNoSql
    {
        try
        {
            if (!_isExternalTransaction && _session is null)
            {
                _session = await _client.StartSessionAsync(null, cToken);
                _session.StartTransaction();
            }

            var collection = GetCollection<T>();

            T[] documents;

            if (options.Data is not null)
            {
                documents = options.Data;
            }
            else
            {
                IQueryable<T> query = collection.AsQueryable();

                options.QueryOptions.BuildQuery(ref query);

                documents = query.ToArray();
            }

            if (documents.Length == 0)
                return documents;

            var replaceOptions = new ReplaceOptions
            {
                IsUpsert = true
            };

            foreach (var document in documents)
            {
                Expression<Func<T, bool>> updateFilter = options.Update(document);

                _ = await collection.ReplaceOneAsync(_session, updateFilter, document, replaceOptions, cToken);
            }

            if (!_isExternalTransaction && _session?.IsInTransaction is true)
                await _session.CommitTransactionAsync(cToken);

            return documents;
        }
        catch
        {
            if (!_isExternalTransaction && _session?.IsInTransaction is true)
                await _session.AbortTransactionAsync(cToken);

            throw;
        }
        finally
        {
            if (!_isExternalTransaction)
                Dispose();
        }
    }

    public async Task<long> Delete<T>(PersistenceQueryOptions<T> options, CancellationToken cToken) where T : class, IPersistent, IPersistentNoSql
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

            if (documents.Length == 0)
                return 0;

            var deleteOptions = new DeleteOptions
            {

            };


            for (int i = 0; i < documents.Length - 1; i++)
            {
                await collection.DeleteOneAsync(_session, options.Filter, deleteOptions, cToken);
            }

            if (!_isExternalTransaction && _session?.IsInTransaction is true)
                await _session.CommitTransactionAsync(cToken);

            return documents.Length;
        }
        catch
        {
            if (!_isExternalTransaction && _session?.IsInTransaction is true)
                await _session.AbortTransactionAsync(cToken);

            throw;
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
            throw new InvalidOperationException("The transaction session was not found");

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
        catch
        {
            throw;
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

    public IMongoCollection<T> SetCollection<T>(CreateCollectionOptions? options = null) where T : class, IPersistent, IPersistentNoSql
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
    public IMongoCollection<T> SetCollection<T>(IEnumerable<T> items, CreateCollectionOptions? options = null) where T : class, IPersistent, IPersistentNoSql
    {
        var collection = SetCollection<T>(options);

        if (items.Any() && collection.CountDocuments(new BsonDocument()) == 0)
            collection.InsertMany(items);

        return collection;
    }
}
