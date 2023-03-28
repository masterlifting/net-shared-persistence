using Net.Shared.Persistence.Abstractions.Entities;

namespace Net.Shared.Persistence.Abstractions.Repositories.Sql;

public interface IPersistenceSqlWriterRepository<T> : IPersistenceWriterRepository<T> where T : class, IPersistentSql
{
}