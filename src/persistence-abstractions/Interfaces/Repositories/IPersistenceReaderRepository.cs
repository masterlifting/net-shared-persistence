using Net.Shared.Persistence.Abstractions.Interfaces.Entities;
using Net.Shared.Persistence.Abstractions.Interfaces.Entities.Catalogs;
using Net.Shared.Persistence.Abstractions.Models.Contexts;

namespace Net.Shared.Persistence.Abstractions.Interfaces.Repositories;

public interface IPersistenceReaderRepository<TEntity> where TEntity : IPersistent
{
    Task<bool> IsExists<T>(PersistenceQueryOptions<T> options, CancellationToken cToken) where T : class, TEntity;
    Task<T?> FindSingle<T>(PersistenceQueryOptions<T> options, CancellationToken cToken) where T : class, TEntity;
    Task<T?> FindFirst<T>(PersistenceQueryOptions<T> options, CancellationToken cToken) where T : class, TEntity;
    Task<T[]> FindMany<T>(PersistenceQueryOptions<T> options, CancellationToken cToken) where T : class, TEntity;
    Task<TResult[]> FindMany<T, TResult>(PersistenceSelectorOptions<T, TResult> options, CancellationToken cToken) where T : class, TEntity;

    #region Catalogs API
    Task<T[]> GetCatalogs<T>(CancellationToken cToken) where T : class, IPersistentCatalog, TEntity;

    Task<T> GetCatalogById<T>(int id, CancellationToken cToken) where T : class, IPersistentCatalog, TEntity;
    Task<Dictionary<int, T>> GetCatalogsDictionaryById<T>(CancellationToken cToken) where T : class, IPersistentCatalog, TEntity;

    Task<T> GetCatalogByName<T>(string name, CancellationToken cToken) where T : class, IPersistentCatalog, TEntity;
    Task<Dictionary<string, T>> GetCatalogsDictionaryByName<T>(CancellationToken cToken) where T : class, IPersistentCatalog, TEntity;

    Task<T> GetCatalogByEnum<T, TEnum>(TEnum value, CancellationToken cToken) where T : class, IPersistentCatalog, TEntity where TEnum : Enum;
    Task<Dictionary<TEnum, T>> GetCatalogsDictionaryByEnum<T, TEnum>(CancellationToken cToken) where T : class, IPersistentCatalog, TEntity where TEnum : Enum;
    #endregion
}
