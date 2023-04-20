using Net.Shared.Persistence.Abstractions.Contexts;
using Net.Shared.Persistence.Abstractions.Entities;

namespace Net.Shared.Persistence.Abstractions.Repositories.NoSql;

public interface IPersistenceNoSqlReaderRepository : IPersistenceReaderRepository<IPersistentNoSql>
{
    IPersistenceNoSqlContext Context { get; }
}