using Net.Shared.Persistence.Abstractions.Entities;

namespace Net.Shared.Persistence.Abstractions.Repositories;

public interface IPersistenceSqlRepository<T> : IPersistenceRepository<T> where T : IPersistentSql
{
}