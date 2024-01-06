using Net.Shared.Persistence.Abstractions.Interfaces.Contexts;
using Net.Shared.Persistence.Abstractions.Interfaces.Entities;

namespace Net.Shared.Persistence.Abstractions.Interfaces.Repositories.Sql;

public interface IPersistenceSqlReaderRepository : IPersistenceReaderRepository<IPersistentSql>
{
    IPersistenceSqlContext Context { get; }

    Task<T?> FindById<T>(object[] id, CancellationToken cToken = default) where T : class, IPersistentSql;
    Task<T?> FindById<T>(object id, CancellationToken cToken = default) where T : class, IPersistentSql;
}