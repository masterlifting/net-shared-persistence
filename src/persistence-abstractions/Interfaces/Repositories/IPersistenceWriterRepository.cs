using Net.Shared.Abstractions.Models.Data;
using Net.Shared.Persistence.Abstractions.Interfaces.Entities;
using Net.Shared.Persistence.Abstractions.Models.Contexts;

namespace Net.Shared.Persistence.Abstractions.Interfaces.Repositories;

public interface IPersistenceWriterRepository<TEntity>
{
    Task CreateOne<T>(T entity, CancellationToken cToken = default) where T : class, IPersistent, TEntity;
    Task<Result<T>> TryCreateOne<T>(T entity, CancellationToken cToken = default) where T : class, IPersistent, TEntity;

    Task CreateMany<T>(IReadOnlyCollection<T> entities, CancellationToken cToken = default) where T : class, IPersistent, TEntity;
    Task<Result<T>> TryCreateMany<T>(IReadOnlyCollection<T> entities, CancellationToken cToken = default) where T : class, IPersistent, TEntity;

    Task<T[]> Update<T>(PersistenceUpdateOptions<T> options, CancellationToken cToken = default) where T : class, IPersistent, TEntity;
    Task<Result<T>> TryUpdate<T>(PersistenceUpdateOptions<T> options, CancellationToken cToken = default) where T : class, IPersistent, TEntity;

    Task<long> Delete<T>(PersistenceQueryOptions<T> options, CancellationToken cToken = default) where T : class, IPersistent, TEntity;
    Task<Result<long>> TryDelete<T>(PersistenceQueryOptions<T> options, CancellationToken cToken = default) where T : class, IPersistent, TEntity;
}
