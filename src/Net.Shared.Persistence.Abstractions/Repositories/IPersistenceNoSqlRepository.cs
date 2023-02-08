using Net.Shared.Persistence.Abstractions.Entities;

namespace Net.Shared.Persistence.Abstractions.Repositories;

public interface IPersistenceNoSqlRepository<T> : IPersistenceRepository<T> where T : class, IPersistentNoSql
{
}