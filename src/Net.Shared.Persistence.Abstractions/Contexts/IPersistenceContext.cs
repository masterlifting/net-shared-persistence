using Net.Shared.Persistence.Abstractions.Entities;
using Net.Shared.Persistence.Models.Contexts;

namespace Net.Shared.Persistence.Abstractions.Contexts;

public interface IPersistenceContext<TEntity> : IDisposable where TEntity : IPersistent
{
    IQueryable<T> GetQuery<T>() where T : class, TEntity;

    Task<bool> IsExists<T>(PersistenceQueryOptions<T> options, CancellationToken cToken) where T : class, TEntity;
    
    Task<T?> FindFirst<T>(PersistenceQueryOptions<T> options, CancellationToken cToken) where T : class, TEntity;
    Task<T?> FindSingle<T>(PersistenceQueryOptions<T> options, CancellationToken cToken) where T : class, TEntity;
    
    Task<T[]> FindMany<T>(PersistenceQueryOptions<T> options, CancellationToken cToken) where T : class, TEntity;
    Task<TResult[]> FindMany<T, TResult>(PersistenceSelectorOptions<T, TResult> options, CancellationToken cToken) where T : class, TEntity;

    Task CreateOne<T>(T entity, CancellationToken cToken) where T : class, TEntity;
    Task CreateMany<T>(IReadOnlyCollection<T> entities, CancellationToken cToken) where T : class, TEntity;

    Task<T[]> Update<T>(PersistenceQueryOptions<T> options, Action<T> updater, CancellationToken cToken) where T : class, TEntity;
    Task Update<T>(PersistenceQueryOptions<T> options, IEnumerable<T> data, CancellationToken cToken) where T : class, TEntity;

    Task<T[]> Delete<T>(PersistenceQueryOptions<T> options, CancellationToken cToken) where T : class, TEntity;

    Task StartTransaction(CancellationToken cToken);
    Task CommitTransaction(CancellationToken cToken);
    Task RollbackTransaction(CancellationToken cToken);
}
