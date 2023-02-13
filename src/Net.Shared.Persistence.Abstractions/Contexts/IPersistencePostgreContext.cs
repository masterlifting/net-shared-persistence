using System.Linq.Expressions;

using Net.Shared.Persistence.Abstractions.Entities;

namespace Net.Shared.Persistence.Abstractions.Contexts
{
    public interface IPersistencePostgreContext : IPersistenceContext<IPersistentSql>
    {
        Task<T?> FindByIdAsync<T>(CancellationToken cToken = default, object[] id) where T : class, IPersistentSql;
        Task<T[]> UpdateAsync<T>(Expression<Func<T, bool>> condition, T entity, CancellationToken cToken = default) where T : class, IPersistentSql;
    }
}
