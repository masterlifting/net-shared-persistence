using Net.Shared.Persistence.Abstractions.Entities;

namespace Net.Shared.Persistence.Abstractions.Repositories.Sql;

public interface IPersistenceSqlReaderRepository<T> : IPersistenceReaderRepository<T> where T : class, IPersistentSql
{
}