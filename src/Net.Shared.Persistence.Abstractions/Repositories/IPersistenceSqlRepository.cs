using Shared.Persistence.Abstractions.Entities;

namespace Shared.Persistence.Abstractions.Repositories;

public interface IPersistenceSqlRepository<T> : IPersistenceRepository<T> where T : class, IPersistentSql
{
}