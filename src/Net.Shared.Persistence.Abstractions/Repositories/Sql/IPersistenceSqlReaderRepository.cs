using Net.Shared.Persistence.Abstractions.Contexts;
using Net.Shared.Persistence.Abstractions.Entities;

namespace Net.Shared.Persistence.Abstractions.Repositories.Sql;

public interface IPersistenceSqlReaderRepository : IPersistenceReaderRepository<IPersistentSql>
{
    IPersistenceSqlContext Context { get; }

    Task<T?> FindById<T>(object[] id, CancellationToken cToken) where T : class, IPersistentSql;
    Task<T?> FindById<T>(object id, CancellationToken cToken) where T : class, IPersistentSql;
}