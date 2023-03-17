using Net.Shared.Persistence.Abstractions.Entities;

using System.Linq.Expressions;

namespace Net.Shared.Persistence.Abstractions.Core.Contexts;

public interface IPersistenceContext<TEntity> : IDisposable where TEntity : IPersistent
{
    IQueryable<T> Set<T>() where T : class, TEntity;

    Task<T[]> FindMany<T>(Expression<Func<T, bool>> filter, CancellationToken cToken = default) where T : class, TEntity;
    Task<T?> FindFirst<T>(Expression<Func<T, bool>> filter, CancellationToken cToken = default) where T : class, TEntity;
    Task<T?> FindSingle<T>(Expression<Func<T, bool>> filter, CancellationToken cToken = default) where T : class, TEntity;

    Task CreateOne<T>(T entity, CancellationToken cToken = default) where T : class, TEntity;
    Task CreateMany<T>(IReadOnlyCollection<T> entities, CancellationToken cToken = default) where T : class, TEntity;

    Task<T[]> Update<T>(Expression<Func<T, bool>> filter, Action<T> updaters, CancellationToken cToken = default) where T : class, TEntity;
    Task<T[]> Update<T>(Expression<Func<T, bool>> filter, T entity, CancellationToken cToken = default) where T : class, TEntity;

    Task<T[]> Delete<T>(Expression<Func<T, bool>> filter, CancellationToken cToken = default) where T : class, TEntity;

    Task StartTransaction(CancellationToken cToken = default);
    Task CommitTransaction(CancellationToken cToken = default);
    Task RollbackTransaction(CancellationToken cToken = default);
}
