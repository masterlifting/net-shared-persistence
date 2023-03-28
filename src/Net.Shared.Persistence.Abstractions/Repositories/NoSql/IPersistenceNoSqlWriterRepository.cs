using Net.Shared.Persistence.Abstractions.Entities;

namespace Net.Shared.Persistence.Abstractions.Repositories.NoSql;

public interface IPersistenceNoSqlWriterRepository<T> : IPersistenceWriterRepository<T> where T : class, IPersistentNoSql
{
}