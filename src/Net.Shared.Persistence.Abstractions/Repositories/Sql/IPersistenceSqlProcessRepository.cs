using Net.Shared.Persistence.Abstractions.Contexts;
using Net.Shared.Persistence.Abstractions.Entities;

namespace Net.Shared.Persistence.Abstractions.Repositories.Sql;

public interface IPersistenceSqlProcessRepository : IPersistenceProcessRepository<IPersistentSql>
{
    IPersistenceSqlContext Context { get; }
}
