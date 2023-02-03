using Shared.Persistence.Abstractions.Entities;

namespace Shared.Persistence.Abstractions.Repositories;

public interface IPersistenceNoSqlRepository<T> : IPersistenceRepository<T> where T : class, IPersistentNoSql
{
}