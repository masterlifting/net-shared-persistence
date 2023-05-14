using System.Linq.Expressions;
using Net.Shared.Persistence.Abstractions.Contexts;
using Net.Shared.Persistence.Abstractions.Entities;

namespace Net.Shared.Persistence.Abstractions.Repositories.NoSql;

public interface IPersistenceNoSqlWriterRepository : IPersistenceWriterRepository<IPersistentNoSql>
{
    IPersistenceNoSqlContext Context { get; }
    Task<T[]> Update<T>(Expression<Func<T, bool>> filter, Action<T> updater, int limit, CancellationToken cToken) where T : class, IPersistentNoSql;
}