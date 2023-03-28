using Net.Shared.Persistence.Abstractions.Entities;

namespace Net.Shared.Persistence.Abstractions.Repositories.NoSql;

public interface IPersistenceNoSqlReaderRepository<T> : IPersistenceReaderRepository<T> where T : class, IPersistentNoSql
{
}