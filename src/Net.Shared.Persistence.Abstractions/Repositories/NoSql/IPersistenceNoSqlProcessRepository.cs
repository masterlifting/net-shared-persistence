using Net.Shared.Persistence.Abstractions.Contexts;
using Net.Shared.Persistence.Abstractions.Entities;

namespace Net.Shared.Persistence.Abstractions.Repositories;

public interface IPersistenceNoSqlProcessRepository : IPersistenceProcessRepository<IPersistentNoSql>
{
    IPersistenceNoSqlContext Context { get; }
}
