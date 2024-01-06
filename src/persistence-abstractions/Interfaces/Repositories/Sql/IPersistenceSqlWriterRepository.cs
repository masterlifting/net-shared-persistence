using Net.Shared.Persistence.Abstractions.Interfaces.Contexts;
using Net.Shared.Persistence.Abstractions.Interfaces.Entities;

namespace Net.Shared.Persistence.Abstractions.Interfaces.Repositories.Sql;

public interface IPersistenceSqlWriterRepository : IPersistenceWriterRepository<IPersistentSql>
{
    IPersistenceSqlContext Context { get; }

    Task UpdateOne<T>(T entity, CancellationToken cToken = default) where T : class, IPersistentSql;
    Task UpdateMany<T>(IEnumerable<T> entities, CancellationToken cToken = default) where T : class, IPersistentSql;

    Task DeleteOne<T>(T entity, CancellationToken cToken = default) where T : class, IPersistentSql;
    Task DeleteMany<T>(IEnumerable<T> entities, CancellationToken cToken = default) where T : class, IPersistentSql;
}