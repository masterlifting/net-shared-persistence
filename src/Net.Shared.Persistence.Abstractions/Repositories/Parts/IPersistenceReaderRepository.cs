using Shared.Persistence.Abstractions.Entities;
using Shared.Persistence.Abstractions.Entities.Catalogs;

using System.Linq.Expressions;

namespace Shared.Persistence.Abstractions.Repositories.Parts
{
    public interface IPersistenceReaderRepository<TEntity> where TEntity : IPersistent
    {
        Task<T?> FindSingleAsync<T>(Expression<Func<T, bool>> condition, CancellationToken cToken = default) where T : class, TEntity;
        Task<T?> FindFirstAsync<T>(Expression<Func<T, bool>> condition, CancellationToken cToken = default) where T : class, TEntity;
        Task<T[]> FindManyAsync<T>(Expression<Func<T, bool>> condition, CancellationToken cToken = default) where T : class, TEntity;

        Task<T[]> GetCatalogsAsync<T>(CancellationToken cToken = default) where T : class, TEntity, IPersistentCatalog;
        Task<Dictionary<int, T>> GetCatalogsDictionaryByIdAsync<T>(CancellationToken cToken = default) where T : class, TEntity, IPersistentCatalog;
        Task<Dictionary<string, T>> GetCatalogsDictionaryByNameAsync<T>(CancellationToken cToken = default) where T : class, TEntity, IPersistentCatalog;
        Task<T?> GetCatalogByIdAsync<T>(int id, CancellationToken cToken = default) where T : class, TEntity, IPersistentCatalog;
        Task<T?> GetCatalogByNameAsync<T>(string name, CancellationToken cToken = default) where T : class, TEntity, IPersistentCatalog;

        Task<T[]> GetProcessableAsync<T>(IProcessStep step, int limit, CancellationToken cToken = default) where T : class, TEntity, IPersistentProcess;
        Task<T[]> GetUnprocessableAsync<T>(IProcessStep step, int limit, DateTime updateTime, int maxAttempts, CancellationToken cToken = default) where T : class, TEntity, IPersistentProcess;
    }
}
