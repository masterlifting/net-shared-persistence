using Net.Shared.Models.Domain;
using Net.Shared.Persistence.Abstractions.Entities;
using Net.Shared.Persistence.Abstractions.Entities.Catalogs;

using System.Linq.Expressions;

namespace Net.Shared.Persistence.Abstractions.Core.Repositories.Parts;

public interface IPersistenceWriterRepository<TEntity> where TEntity : class, IPersistent
{
    Task CreateOne<T>(T entity, CancellationToken cToken = default) where T : class, TEntity;
    Task CreateMany<T>(IReadOnlyCollection<T> entities, CancellationToken cToken = default) where T : class, TEntity;
    
    Task<TryResult<T>> TryCreateOne<T>(T entity, CancellationToken cToken = default) where T : class, TEntity;
    Task<TryResult<T[]>> TryCreateMany<T>(IReadOnlyCollection<T> entities, CancellationToken cToken = default) where T : class, TEntity;

    Task<T[]> Update<T>(Expression<Func<T, bool>> filter, Action<T> updaters, CancellationToken cToken = default) where T : class, TEntity;
    Task<TryResult<T[]>> TryUpdate<T>(Expression<Func<T, bool>> condition, Action<T> updaters, CancellationToken cToken = default) where T : class, TEntity;
    Task<T[]> Update<T>(Expression<Func<T, bool>> filter, T entity, CancellationToken cToken = default) where T : class, TEntity;
    Task<TryResult<T[]>> TryUpdate<T>(Expression<Func<T, bool>> filter, T entity, CancellationToken cToken = default) where T : class, TEntity;

    Task<T[]> Delete<T>(Expression<Func<T, bool>> filter, CancellationToken cToken = default) where T : class, TEntity;
    Task<TryResult<T[]>> TryDelete<T>(Expression<Func<T, bool>> filter, CancellationToken cToken = default) where T : class, TEntity;

    Task SaveProcessableData<T>(IPersistentProcessStep? step, IEnumerable<T> entities, CancellationToken cToken) where T : class, TEntity, IPersistentProcess;
}
