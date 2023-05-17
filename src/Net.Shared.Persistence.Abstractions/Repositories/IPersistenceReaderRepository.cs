using Net.Shared.Persistence.Abstractions.Entities;
using Net.Shared.Persistence.Abstractions.Entities.Catalogs;
using Net.Shared.Persistence.Models.Contexts;

namespace Net.Shared.Persistence.Abstractions.Repositories;

public interface IPersistenceReaderRepository<TEntity> where TEntity : class, IPersistent
{
    Task<T?> FindSingle<T>(PersistenceQueryOptions<T> options, CancellationToken cToken) where T : class, TEntity;
    Task<T?> FindFirst<T>(PersistenceQueryOptions<T> options, CancellationToken cToken) where T : class, TEntity;
    Task<T[]> FindMany<T>(PersistenceQueryOptions<T> options, CancellationToken cToken) where T : class, TEntity;

    Task<T[]> GetCatalogs<T>(CancellationToken cToken) where T : class, TEntity, IPersistentCatalog;
    Task<Dictionary<int, T>> GetCatalogsDictionaryById<T>(CancellationToken cToken) where T : class, TEntity, IPersistentCatalog;
    Task<Dictionary<string, T>> GetCatalogsDictionaryByName<T>(CancellationToken cToken) where T : class, TEntity, IPersistentCatalog;
    Task<T?> GetCatalogById<T>(int id, CancellationToken cToken) where T : class, TEntity, IPersistentCatalog;
    Task<T?> GetCatalogByName<T>(string name, CancellationToken cToken) where T : class, TEntity, IPersistentCatalog;
}
