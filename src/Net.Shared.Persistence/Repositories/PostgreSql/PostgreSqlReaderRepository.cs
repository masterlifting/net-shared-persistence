using System.Linq.Expressions;
using Net.Shared.Persistence.Abstractions.Entities;
using Net.Shared.Persistence.Abstractions.Entities.Catalogs;
using Net.Shared.Persistence.Abstractions.Repositories.Sql;
using Net.Shared.Persistence.Contexts;

namespace Net.Shared.Persistence.Repositories.PostgreSql;

public sealed class PostgreSqlReaderRepository<TEntity> : IPersistenceSqlReaderRepository<TEntity> where TEntity : class, IPersistentSql
{
    private readonly PostgreSqlContext _context;
    public PostgreSqlReaderRepository(PostgreSqlContext context) => _context = context;

    public Task<T?> FindSingle<T>(Expression<Func<T, bool>> filter, CancellationToken cToken) where T : class, TEntity =>
        _context.FindSingle(filter, cToken);
    public Task<T?> FindFirst<T>(Expression<Func<T, bool>> filter, CancellationToken cToken) where T : class, TEntity =>
        _context.FindFirst(filter, cToken);
    public Task<T[]> FindMany<T>(Expression<Func<T, bool>> filter, CancellationToken cToken) where T : class, TEntity =>
        _context.FindMany(filter, cToken);

    public Task<T[]> GetCatalogs<T>(CancellationToken cToken) where T : class, IPersistentCatalog, TEntity =>
        _context.FindMany<T>(x => true, cToken);
    public Task<T?> GetCatalogById<T>(int id, CancellationToken cToken) where T : class, IPersistentCatalog, TEntity =>
        _context.FindSingle<T>(x => x.Id == id, cToken);
    public Task<T?> GetCatalogByName<T>(string name, CancellationToken cToken) where T : class, IPersistentCatalog, TEntity =>
        _context.FindSingle<T>(x => x.Name.Equals(name), cToken);
    public Task<Dictionary<int, T>> GetCatalogsDictionaryById<T>(CancellationToken cToken) where T : class, IPersistentCatalog, TEntity =>
            Task.Run(() => _context.SetEntity<T>().ToDictionary(x => x.Id));
    public Task<Dictionary<string, T>> GetCatalogsDictionaryByName<T>(CancellationToken cToken) where T : class, IPersistentCatalog, TEntity =>
            Task.Run(() => _context.SetEntity<T>().ToDictionary(x => x.Name));
}
