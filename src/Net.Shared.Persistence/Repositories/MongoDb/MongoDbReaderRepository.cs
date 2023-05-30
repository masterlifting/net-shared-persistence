using Net.Shared.Persistence.Abstractions.Contexts;
using Net.Shared.Persistence.Abstractions.Entities;
using Net.Shared.Persistence.Abstractions.Entities.Catalogs;
using Net.Shared.Persistence.Abstractions.Repositories.NoSql;
using Net.Shared.Persistence.Contexts;
using Net.Shared.Persistence.Models.Contexts;
using Net.Shared.Persistence.Models.Exceptions;

namespace Net.Shared.Persistence.Repositories.MongoDb;

public sealed class MongoDbReaderRepository : IPersistenceNoSqlReaderRepository
{
    public MongoDbReaderRepository(MongoDbContext context)
    {
        _context = context;
        Context = context;
    }

    #region PRIVATE FIELDS
    private readonly MongoDbContext _context;
    #endregion

    #region PUBLIC PROPERTIES
    public IPersistenceNoSqlContext Context { get; }
    #endregion

    #region PUBLIC METHODS

    public Task<T?> FindSingle<T>(PersistenceQueryOptions<T> options, CancellationToken cToken) where T : class, IPersistentNoSql
    {
        options.Take = 2;
        return _context.FindSingle(options, cToken);
    }

    public Task<T?> FindFirst<T>(PersistenceQueryOptions<T> options, CancellationToken cToken) where T : class, IPersistentNoSql
    {
        options.Take = 1;
        return _context.FindFirst(options, cToken);
    }

    public Task<T[]> FindMany<T>(PersistenceQueryOptions<T> options, CancellationToken cToken) where T : class, IPersistentNoSql =>
        _context.FindMany(options, cToken);
    public Task<bool> IsExists<T>(PersistenceQueryOptions<T> options, CancellationToken cToken) where T : class, IPersistentNoSql
    {
        options.Take = 1;
        return _context.FindFirst(options, cToken).ContinueWith(x => x.Result is not null);
    }

    public Task<T[]> GetCatalogs<T>(CancellationToken cToken) where T : class, IPersistentCatalog, IPersistentNoSql =>
        _context.FindMany<T>(new() { Filter = _ => true }, cToken);
    public async Task<T> GetCatalogById<T>(int id, CancellationToken cToken) where T : class, IPersistentCatalog, IPersistentNoSql
    {
        var options = new PersistenceQueryOptions<T>
        {
            Filter = x => x.Id == id,
            Take = 2
        };
        return await _context.FindSingle(options, cToken) ?? throw new PersistenceException($"Catalog {typeof(T).Name} with id {id} not found");
    }

    public async Task<T> GetCatalogByName<T>(string name, CancellationToken cToken) where T : class, IPersistentCatalog, IPersistentNoSql
    {
        var options = new PersistenceQueryOptions<T>
        {
            Filter = x => x.Name.Equals(name),
            Take = 2
        };
        return await _context.FindSingle(options, cToken) ?? throw new PersistenceException($"Catalog {typeof(T).Name} with name {name} not found");
    }

    public Task<Dictionary<int, T>> GetCatalogsDictionaryById<T>(CancellationToken cToken) where T : class, IPersistentCatalog, IPersistentNoSql =>
            Task.Run(() => _context.SetIQueryable<T>().ToDictionary(x => x.Id));
    public Task<Dictionary<string, T>> GetCatalogsDictionaryByName<T>(CancellationToken cToken) where T : class, IPersistentCatalog, IPersistentNoSql =>
            Task.Run(() => _context.SetIQueryable<T>().ToDictionary(x => x.Name));
    public async Task<T> GetCatalogByEnum<T, TEnum>(TEnum value, CancellationToken cToken)
        where T : class, IPersistentCatalog, IPersistentNoSql
        where TEnum : Enum
    {
        var name = Enum.GetName(typeof(TEnum), value);

        return name is null
            ? throw new PersistenceException($"Enum {typeof(TEnum).Name} does not contain value {value}")
            : await GetCatalogByName<T>(name, cToken);
    }

    #endregion
}
