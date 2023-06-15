using Net.Shared.Models.Domain;
using Net.Shared.Persistence.Abstractions.Entities;
using Net.Shared.Persistence.Models.Contexts;

namespace Net.Shared.Persistence.Abstractions.Repositories;

public interface IPersistenceWriterRepository<TEntity> where TEntity : class, IPersistent
{
    Task CreateOne<T>(T entity, CancellationToken cToken = default) where T : class, TEntity;
    Task<Result<T>> TryCreateOne<T>(T entity, CancellationToken cToken = default) where T : class, TEntity;

    Task CreateMany<T>(IReadOnlyCollection<T> entities, CancellationToken cToken = default) where T : class, TEntity;
    Task<Result<T>> TryCreateMany<T>(IReadOnlyCollection<T> entities, CancellationToken cToken = default) where T : class, TEntity;

    Task<T[]> Update<T>(PersistenceQueryOptions<T> options, Action<T> updater, CancellationToken cToken = default) where T : class, TEntity;
    Task<Result<T>> TryUpdate<T>(PersistenceQueryOptions<T> options, Action<T> updater, CancellationToken cToken = default) where T : class, TEntity;

    Task<long> Delete<T>(PersistenceQueryOptions<T> options, CancellationToken cToken = default) where T : class, TEntity;
    Task<Result<long>> TryDelete<T>(PersistenceQueryOptions<T> options, CancellationToken cToken = default) where T : class, TEntity;
}
