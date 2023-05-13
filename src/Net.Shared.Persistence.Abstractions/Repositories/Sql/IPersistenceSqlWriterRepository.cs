using Net.Shared.Persistence.Abstractions.Contexts;
using Net.Shared.Persistence.Abstractions.Entities;

namespace Net.Shared.Persistence.Abstractions.Repositories.Sql;

public interface IPersistenceSqlWriterRepository : IPersistenceWriterRepository<IPersistentSql>
{
    IPersistenceSqlContext Context { get; }

    Task UpdateOne<T>(T entity, CancellationToken cToken) where T : class, IPersistentSql;
    Task UpdateMany<T>(IEnumerable<T> entities, CancellationToken cToken) where T : class, IPersistentSql;

    Task DeleteOne<T>(T entity, CancellationToken cToken) where T : class, IPersistentSql;
    Task DeleteMany<T>(IEnumerable<T> entities, CancellationToken cToken) where T : class, IPersistentSql;
}