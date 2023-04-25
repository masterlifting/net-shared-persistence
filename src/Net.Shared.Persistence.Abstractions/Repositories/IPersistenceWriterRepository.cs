using System.Linq.Expressions;

using Net.Shared.Models.Domain;
using Net.Shared.Persistence.Abstractions.Entities;

namespace Net.Shared.Persistence.Abstractions.Repositories;

public interface IPersistenceWriterRepository<TEntity> where TEntity : class, IPersistent
{
    Task CreateOne<T>(T entity, CancellationToken cToken) where T : class, TEntity;
    Task<Result<T>> TryCreateOne<T>(T entity, CancellationToken cToken) where T : class, TEntity;

    Task CreateMany<T>(IReadOnlyCollection<T> entities, CancellationToken cToken) where T : class, TEntity;
    Task<Result<T>> TryCreateMany<T>(IReadOnlyCollection<T> entities, CancellationToken cToken) where T : class, TEntity;

    Task<T[]> Update<T>(Expression<Func<T, bool>> filter, Action<T> updater, CancellationToken cToken) where T : class, TEntity;
    Task<Result<T>> TryUpdate<T>(Expression<Func<T, bool>> filter, Action<T> updater, CancellationToken cToken) where T : class, TEntity;

    Task<T[]> Delete<T>(Expression<Func<T, bool>> filter, CancellationToken cToken) where T : class, TEntity;
    Task<Result<T>> TryDelete<T>(Expression<Func<T, bool>> filter, CancellationToken cToken) where T : class, TEntity;
}
