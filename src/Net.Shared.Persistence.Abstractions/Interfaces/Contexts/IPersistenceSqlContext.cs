using Net.Shared.Persistence.Abstractions.Interfaces.Entities;

namespace Net.Shared.Persistence.Abstractions.Interfaces.Contexts;

public interface IPersistenceSqlContext : IPersistenceContext<IPersistentSql>
{
    string GetTableName<T>() where T : class, IPersistentSql;

    IQueryable<T> GetQueryFromRaw<T>(FormattableString query, CancellationToken cToken) where T : class, IPersistentSql;

    Task<T?> FindById<T>(object[] id, CancellationToken cToken) where T : class, IPersistentSql;
    Task<T?> FindById<T>(object id, CancellationToken cToken) where T : class, IPersistentSql;

    Task UpdateOne<T>(T entity, CancellationToken cToken) where T : class, IPersistentSql;
    Task UpdateMany<T>(IEnumerable<T> entities, CancellationToken cToken) where T : class, IPersistentSql;

    Task DeleteOne<T>(T entity, CancellationToken cToken) where T : class, IPersistentSql;
    Task DeleteMany<T>(IEnumerable<T> entities, CancellationToken cToken) where T : class, IPersistentSql;
}
