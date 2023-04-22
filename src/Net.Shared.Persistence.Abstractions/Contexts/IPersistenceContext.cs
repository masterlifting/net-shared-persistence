using System.Linq.Expressions;
using Net.Shared.Persistence.Abstractions.Entities;

namespace Net.Shared.Persistence.Abstractions.Contexts;

public interface IPersistenceContext<TEntity> : IDisposable where TEntity : IPersistent
{
    IQueryable<T> SetEntity<T>() where T : class, TEntity;

    Task<T[]> FindAll<T>(CancellationToken cToken) where T : class, TEntity;
    Task<T[]> FindMany<T>(Expression<Func<T, bool>> filter, CancellationToken cToken) where T : class, TEntity;
    Task<T?> FindFirst<T>(Expression<Func<T, bool>> filter, CancellationToken cToken) where T : class, TEntity;
    Task<T?> FindSingle<T>(Expression<Func<T, bool>> filter, CancellationToken cToken) where T : class, TEntity;

    Task CreateOne<T>(T entity, CancellationToken cToken) where T : class, TEntity;
    Task CreateMany<T>(IReadOnlyCollection<T> entities, CancellationToken cToken) where T : class, TEntity;

    Task<T[]> Update<T>(Expression<Func<T, bool>> filter, Action<T> updater, CancellationToken cToken) where T : class, TEntity;

    Task<T[]> Delete<T>(Expression<Func<T, bool>> filter, CancellationToken cToken) where T : class, TEntity;

    Task StartTransaction(CancellationToken cToken);
    Task CommitTransaction(CancellationToken cToken);
    Task RollbackTransaction(CancellationToken cToken);
}
