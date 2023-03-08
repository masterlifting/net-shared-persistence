using Net.Shared.Persistence.Abstractions.Entities;

namespace Net.Shared.Persistence.Abstractions.Core.Repositories;

public interface IPersistenceNoSqlRepository<T> : IPersistenceRepository<T> where T : class, IPersistentNoSql
{
}