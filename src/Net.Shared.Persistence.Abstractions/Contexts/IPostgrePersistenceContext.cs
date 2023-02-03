using System.Linq.Expressions;

using Shared.Persistence.Abstractions.Entities;

namespace Shared.Persistence.Abstractions.Contexts
{
    public interface IPostgrePersistenceContext : IPersistenceContext<IPersistentSql>
    {
        Task<T?> FindByIdAsync<T>(CancellationToken cToken = default, object[] id) where T : class, IPersistentSql;
        Task<T[]> UpdateAsync<T>(Expression<Func<T, bool>> condition, T entity, CancellationToken cToken = default) where T : class, IPersistentSql;
    }
}
