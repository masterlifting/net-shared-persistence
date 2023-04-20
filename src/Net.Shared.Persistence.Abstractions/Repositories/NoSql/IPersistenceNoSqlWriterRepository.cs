using Net.Shared.Persistence.Abstractions.Contexts;
using Net.Shared.Persistence.Abstractions.Entities;

namespace Net.Shared.Persistence.Abstractions.Repositories.NoSql;

public interface IPersistenceNoSqlWriterRepository : IPersistenceWriterRepository<IPersistentNoSql>
{
    IPersistenceNoSqlContext Context { get; }
}