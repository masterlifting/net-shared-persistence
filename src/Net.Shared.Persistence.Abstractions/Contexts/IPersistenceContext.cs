using System.Linq.Expressions;

using Net.Shared.Persistence.Abstractions.Entities;

namespace Net.Shared.Persistence.Abstractions.Contexts
{
    public interface IPersistenceContext<TEntity> : IDisposable where TEntity : IPersistent
    {
        IQueryable<T> Set<T>() where T : class, TEntity;

        Task<T[]> FindManyAsync<T>(Expression<Func<T, bool>> condition, CancellationToken cToken = default) where T : class, TEntity;
        Task<T?> FindFirstAsync<T>(Expression<Func<T, bool>> condition, CancellationToken cToken = default) where T : class, TEntity;
        Task<T?> FindSingleAsync<T>(Expression<Func<T, bool>> condition, CancellationToken cToken = default) where T : class, TEntity;

        Task CreateAsync<T>(T entity, CancellationToken cToken = default) where T : class, TEntity;
        Task CreateManyAsync<T>(IReadOnlyCollection<T> entities, CancellationToken cToken = default) where T : class, TEntity;
        Task<T[]> DeleteAsync<T>(Expression<Func<T, bool>> condition, CancellationToken cToken = default) where T : class, TEntity;

        Task StartTransactionAsync(CancellationToken cToken = default);
        Task CommitTransactionAsync(CancellationToken cToken = default);
        Task RollbackTransactionAsync(CancellationToken cToken = default);
    }
}
