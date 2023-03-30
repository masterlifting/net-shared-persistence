using System.Linq.Expressions;
using Net.Shared.Persistence.Abstractions.Entities;

namespace Net.Shared.Persistence.Abstractions.Contexts;

public interface IPersistenceNoSqlContext : IPersistenceContext<IPersistentNoSql>
{
    Task<T[]> Update<T>(Expression<Func<T, bool>> filter, Action<T> updater, CancellationToken cToken) where T : class, IPersistentNoSql;
}
