using Net.Shared.Persistence.Abstractions.Contexts;
using Net.Shared.Persistence.Abstractions.Entities;

namespace Net.Shared.Persistence.Abstractions.Repositories.Sql;

public interface IPersistenceSqlWriterRepository : IPersistenceWriterRepository<IPersistentSql>
{
    IPersistenceSqlContext Context { get; }
}