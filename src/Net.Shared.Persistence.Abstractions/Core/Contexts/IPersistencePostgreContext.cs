using System.Linq.Expressions;

using Net.Shared.Persistence.Abstractions.Entities;

namespace Net.Shared.Persistence.Abstractions.Core.Contexts
{
    public interface IPersistencePostgreContext : IPersistenceContext<IPersistentSql>
    {
        string GetTableName<T>() where T : class, IPersistentSql;
        IQueryable<T> FromSqlRaw<T>(string sqlQuery) where T : class, IPersistentSql;
        Task<T?> FindById<T>(object[] id, CancellationToken cToken = default) where T : class, IPersistentSql;
        Task<T[]> Update<T>(Expression<Func<T, bool>> filter, T entity, CancellationToken cToken = default) where T : class, IPersistentSql;
    }
}
