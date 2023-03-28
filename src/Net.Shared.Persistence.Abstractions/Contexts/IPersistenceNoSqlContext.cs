using Net.Shared.Persistence.Abstractions.Entities;

namespace Net.Shared.Persistence.Abstractions.Contexts;

public interface IPersistenceNoSqlContext : IPersistenceContext<IPersistentNoSql>
{
}
