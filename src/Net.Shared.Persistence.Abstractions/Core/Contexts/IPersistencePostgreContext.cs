using Net.Shared.Persistence.Abstractions.Entities;

namespace Net.Shared.Persistence.Abstractions.Core.Contexts;

public interface IPersistencePostgreContext : IPersistenceContext<IPersistentSql>
{
    string GetTableName<T>() where T : class, IPersistentSql;
    
    IQueryable<T> GetQueryFromSqlRaw<T>(FormattableString sql) where T : class, IPersistentSql;
    
    Task<T?> FindById<T>(object[] id, CancellationToken cToken = default) where T : class, IPersistentSql;
    
    Task UpdateOne<T>(T entity, CancellationToken cToken = default) where T : class, IPersistentSql;
    Task UpdateMany<T>(IEnumerable<T> entities, CancellationToken cToken = default) where T : class, IPersistentSql;

    Task DeleteOne<T>(T entity, CancellationToken cToken = default) where T : class, IPersistentSql;
    Task DeleteMany<T>(IEnumerable<T> entities, CancellationToken cToken = default) where T : class, IPersistentSql;
}
