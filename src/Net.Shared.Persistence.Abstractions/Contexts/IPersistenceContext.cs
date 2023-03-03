using System.Linq.Expressions;

using Net.Shared.Persistence.Abstractions.Entities;

namespace Net.Shared.Persistence.Abstractions.Contexts
{
    public interface IPersistenceContext<TEntity> : IDisposable where TEntity : IPersistent
    {
        IQueryable<T> Set<T>() where T : class, TEntity;

        Task<T[]> FindMany<T>(Expression<Func<T, bool>> filter, CancellationToken cToken = default) where T : class, TEntity;
        Task<T?> FindFirst<T>(Expression<Func<T, bool>> filter, CancellationToken cToken = default) where T : class, TEntity;
        Task<T?> FindSingle<T>(Expression<Func<T, bool>> filter, CancellationToken cToken = default) where T : class, TEntity;

        Task CreateOne<T>(T entity, CancellationToken cToken = default) where T : class, TEntity;
        Task CreateMany<T>(IReadOnlyCollection<T> entities, CancellationToken cToken = default) where T : class, TEntity;
        Task<T[]> Delete<T>(Expression<Func<T, bool>> filter, CancellationToken cToken = default) where T : class, TEntity;

        Task StartTransaction(CancellationToken cToken = default);
        Task CommitTransaction(CancellationToken cToken = default);
        Task RollbackTransaction(CancellationToken cToken = default);
    }
}
