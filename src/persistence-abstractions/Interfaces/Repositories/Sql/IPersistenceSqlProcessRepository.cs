using Net.Shared.Persistence.Abstractions.Interfaces.Contexts;
using Net.Shared.Persistence.Abstractions.Interfaces.Entities;

namespace Net.Shared.Persistence.Abstractions.Interfaces.Repositories.Sql;

public interface IPersistenceSqlProcessRepository : IPersistenceProcessRepository<IPersistentSql>
{
    IPersistenceSqlContext Context { get; }
}
