using System.Linq.Expressions;
using Net.Shared.Persistence.Abstractions.Entities;
using Net.Shared.Persistence.Abstractions.Entities.Catalogs;

namespace Net.Shared.Persistence.Abstractions.Repositories;

public interface IPersistenceReaderRepository<TEntity> where TEntity : class, IPersistent
{
    Task<T?> FindSingle<T>(Expression<Func<T, bool>> filter, CancellationToken cToken) where T : class, TEntity;
    Task<T?> FindFirst<T>(Expression<Func<T, bool>> filter, CancellationToken cToken) where T : class, TEntity;
    Task<T[]> FindMany<T>(Expression<Func<T, bool>> filter, CancellationToken cToken) where T : class, TEntity;

    Task<T[]> GetCatalogs<T>(CancellationToken cToken) where T : class, TEntity, IPersistentCatalog;
    Task<Dictionary<int, T>> GetCatalogsDictionaryById<T>(CancellationToken cToken) where T : class, TEntity, IPersistentCatalog;
    Task<Dictionary<string, T>> GetCatalogsDictionaryByName<T>(CancellationToken cToken) where T : class, TEntity, IPersistentCatalog;
    Task<T?> GetCatalogById<T>(int id, CancellationToken cToken) where T : class, TEntity, IPersistentCatalog;
    Task<T?> GetCatalogByName<T>(string name, CancellationToken cToken) where T : class, TEntity, IPersistentCatalog;

    Task<T[]> GetProcessableData<T>(IPersistentProcessStep step, int limit, CancellationToken cToken) where T : class, TEntity, IPersistentProcess;
    Task<T[]> GetUnprocessableData<T>(IPersistentProcessStep step, int limit, DateTime updateTime, int maxAttempts, CancellationToken cToken) where T : class, TEntity, IPersistentProcess;
}
