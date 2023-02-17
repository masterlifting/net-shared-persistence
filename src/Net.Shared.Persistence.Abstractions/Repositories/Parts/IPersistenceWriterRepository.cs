using Net.Shared.Models.Results;
using Net.Shared.Persistence.Abstractions.Entities;
using Net.Shared.Persistence.Abstractions.Entities.Catalogs;

using System.Linq.Expressions;

namespace Net.Shared.Persistence.Abstractions.Repositories.Parts
{
    public interface IPersistenceWriterRepository<TEntity> where TEntity : IPersistent
    {
        Task CreateAsync<T>(T entity, CancellationToken cToken = default) where T : class, TEntity;
        Task CreateManyAsync<T>(IReadOnlyCollection<T> entities, CancellationToken cToken = default) where T : class, TEntity;
        Task<TryResult<T>> TryCreateAsync<T>(T entity, CancellationToken cToken = default) where T : class, TEntity;
        Task<TryResult<T[]>> TryCreateManyAsync<T>(IReadOnlyCollection<T> entities, CancellationToken cToken = default) where T : class, TEntity;

        Task<T[]> UpdateAsync<T>(Expression<Func<T, bool>> condition, T entity, CancellationToken cToken = default) where T : class, TEntity;
        Task<TryResult<T[]>> TryUpdateAsync<T>(Expression<Func<T, bool>> condition, T entity, CancellationToken cToken = default) where T : class, TEntity;

        Task<T[]> DeleteAsync<T>(Expression<Func<T, bool>> condition, CancellationToken cToken = default) where T : class, TEntity;
        Task<TryResult<T[]>> TryDeleteAsync<T>(Expression<Func<T, bool>> condition, CancellationToken cToken = default) where T : class, TEntity;

        Task SaveProcessableAsync<T>(IProcessStep? step, IEnumerable<T> entities, CancellationToken cToken) where T : class, TEntity, IPersistentProcess;
    }
}
