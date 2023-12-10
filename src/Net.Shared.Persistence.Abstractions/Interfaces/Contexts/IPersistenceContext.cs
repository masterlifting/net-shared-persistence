using Net.Shared.Persistence.Abstractions.Interfaces.Entities;
using Net.Shared.Persistence.Abstractions.Models.Contexts;

namespace Net.Shared.Persistence.Abstractions.Interfaces.Contexts;

public interface IPersistenceContext<TEntity> : IDisposable
{
    IQueryable<T> GetQuery<T>() where T : class, IPersistent, TEntity;

    Task<bool> IsExists<T>(PersistenceQueryOptions<T> options, CancellationToken cToken) where T : class, IPersistent, TEntity;

    Task<T?> FindFirst<T>(PersistenceQueryOptions<T> options, CancellationToken cToken) where T : class, IPersistent, TEntity;
    Task<T?> FindSingle<T>(PersistenceQueryOptions<T> options, CancellationToken cToken) where T : class, IPersistent, TEntity;

    Task<T[]> FindMany<T>(PersistenceQueryOptions<T> options, CancellationToken cToken) where T : class, IPersistent, TEntity;
    Task<TResult[]> FindMany<T, TResult>(PersistenceSelectorOptions<T, TResult> options, CancellationToken cToken) where T : class, IPersistent, TEntity;

    Task CreateOne<T>(T entity, CancellationToken cToken) where T : class, IPersistent, TEntity;
    Task CreateMany<T>(IReadOnlyCollection<T> entities, CancellationToken cToken) where T : class, IPersistent, TEntity;

    Task<T[]> Update<T>(PersistenceUpdateOptions<T> options, CancellationToken cToken) where T : class, IPersistent, TEntity;

    Task<long> Delete<T>(PersistenceQueryOptions<T> options, CancellationToken cToken) where T : class, IPersistent, TEntity;

    Task StartTransaction(CancellationToken cToken);
    Task CommitTransaction(CancellationToken cToken);
    Task RollbackTransaction(CancellationToken cToken);
}
