using Net.Shared.Persistence.Abstractions.Entities;

namespace Net.Shared.Persistence.Abstractions.Core.Repositories;

public interface IPersistenceSqlRepository<T> : IPersistenceRepository<T> where T : class, IPersistentSql
{
}