using Net.Shared.Persistence.Abstractions.Interfaces.Contexts;
using Net.Shared.Persistence.Abstractions.Interfaces.Entities;

namespace Net.Shared.Persistence.Abstractions.Interfaces.Repositories.NoSql;

public interface IPersistenceNoSqlProcessRepository : IPersistenceProcessRepository<IPersistentNoSql>
{
    IPersistenceNoSqlContext Context { get; }
}
