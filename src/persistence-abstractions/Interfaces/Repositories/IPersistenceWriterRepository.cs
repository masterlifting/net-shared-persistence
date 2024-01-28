using Net.Shared.Abstractions.Models.Data;
using Net.Shared.Persistence.Abstractions.Interfaces.Entities;
using Net.Shared.Persistence.Abstractions.Models.Contexts;

namespace Net.Shared.Persistence.Abstractions.Interfaces.Repositories;

public interface IPersistenceWriterRepository<TEntity> where TEntity : IPersistent
{
    Task CreateOne<T>(T entity, CancellationToken cToken) where T : class, TEntity;
    Task<Result<T>> TryCreateOne<T>(T entity, CancellationToken cToken) where T : class, TEntity;

    Task CreateMany<T>(IReadOnlyCollection<T> entities, CancellationToken cToken) where T : class, TEntity;
    Task<Result<T>> TryCreateMany<T>(IReadOnlyCollection<T> entities, CancellationToken cToken) where T : class, TEntity;

    Task<T[]> Update<T>(PersistenceUpdateOptions<T> options, CancellationToken cToken) where T : class, TEntity;
    Task<Result<T>> TryUpdate<T>(PersistenceUpdateOptions<T> options, CancellationToken cToken) where T : class, TEntity;

    Task<long> Delete<T>(PersistenceQueryOptions<T> options, CancellationToken cToken) where T : class, TEntity;
    Task<Result<long>> TryDelete<T>(PersistenceQueryOptions<T> options, CancellationToken cToken) where T : class, TEntity;
}
