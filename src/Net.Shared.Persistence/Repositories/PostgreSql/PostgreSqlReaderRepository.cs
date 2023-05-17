using Net.Shared.Persistence.Abstractions.Contexts;
using Net.Shared.Persistence.Abstractions.Entities;
using Net.Shared.Persistence.Abstractions.Entities.Catalogs;
using Net.Shared.Persistence.Abstractions.Repositories.Sql;
using Net.Shared.Persistence.Contexts;
using Net.Shared.Persistence.Models.Contexts;

namespace Net.Shared.Persistence.Repositories.PostgreSql;

public sealed class PostgreSqlReaderRepository : IPersistenceSqlReaderRepository
{
    public PostgreSqlReaderRepository(PostgreSqlContext context)
    {
        _context = context;
        Context = context;
    }

    #region PRIVATE FIELDS
    private readonly PostgreSqlContext _context;
    #endregion

    #region PUBLIC PROPERTIES
    public IPersistenceSqlContext Context { get; }
    #endregion

    #region PUBLIC METHODS

    public Task<T?> FindById<T>(object[] id, CancellationToken cToken) where T : class, IPersistentSql =>
        _context.FindById<T>(id, cToken);
    public Task<T?> FindById<T>(object id, CancellationToken cToken) where T : class, IPersistentSql =>
        _context.FindById<T>(id, cToken);
    public Task<T?> FindSingle<T>(PersistenceQueryOptions<T> options, CancellationToken cToken) where T : class, IPersistentSql =>
        _context.FindSingle(options, cToken);
    public Task<T?> FindFirst<T>(PersistenceQueryOptions<T> options, CancellationToken cToken) where T : class, IPersistentSql =>
        _context.FindFirst(options, cToken);
    public Task<T[]> FindMany<T>(PersistenceQueryOptions<T> options, CancellationToken cToken) where T : class, IPersistentSql =>
        _context.FindMany(options, cToken);

    public Task<T[]> GetCatalogs<T>(CancellationToken cToken) where T : class, IPersistentCatalog, IPersistentSql =>
        _context.FindMany<T>(new() { Filter = _ => true }, cToken);
    public Task<T?> GetCatalogById<T>(int id, CancellationToken cToken) where T : class, IPersistentCatalog, IPersistentSql =>
        _context.FindSingle<T>(new() { Filter = x => x.Id == id }, cToken);
    public Task<T?> GetCatalogByName<T>(string name, CancellationToken cToken) where T : class, IPersistentCatalog, IPersistentSql =>
        _context.FindSingle<T>(new() { Filter = x => x.Name.Equals(name) }, cToken);
    public Task<Dictionary<int, T>> GetCatalogsDictionaryById<T>(CancellationToken cToken) where T : class, IPersistentCatalog, IPersistentSql =>
            Task.Run(() => _context.SetEntity<T>().ToDictionary(x => x.Id));
    public Task<Dictionary<string, T>> GetCatalogsDictionaryByName<T>(CancellationToken cToken) where T : class, IPersistentCatalog, IPersistentSql =>
            Task.Run(() => _context.SetEntity<T>().ToDictionary(x => x.Name));

    #endregion
}
