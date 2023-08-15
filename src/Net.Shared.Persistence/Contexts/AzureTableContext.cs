using Net.Shared.Persistence.Abstractions.Contexts;
using Net.Shared.Persistence.Abstractions.Entities;
using Net.Shared.Persistence.Models.Contexts;

namespace Net.Shared.Persistence.Contexts
{
    public abstract class AzureTableContext : IPersistenceNoSqlContext
    {
        public Task CommitTransaction(CancellationToken cToken)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public Task RollbackTransaction(CancellationToken cToken)
        {
            throw new NotImplementedException();
        }

        public Task StartTransaction(CancellationToken cToken)
        {
            throw new NotImplementedException();
        }

        Task IPersistenceContext<IPersistentNoSql>.CreateMany<T>(IReadOnlyCollection<T> entities, CancellationToken cToken)
        {
            throw new NotImplementedException();
        }

        Task IPersistenceContext<IPersistentNoSql>.CreateOne<T>(T entity, CancellationToken cToken)
        {
            throw new NotImplementedException();
        }

        Task<long> IPersistenceContext<IPersistentNoSql>.Delete<T>(PersistenceQueryOptions<T> options, CancellationToken cToken)
        {
            throw new NotImplementedException();
        }

        Task<T?> IPersistenceContext<IPersistentNoSql>.FindFirst<T>(PersistenceQueryOptions<T> options, CancellationToken cToken) where T : class
        {
            throw new NotImplementedException();
        }

        Task<T[]> IPersistenceContext<IPersistentNoSql>.FindMany<T>(PersistenceQueryOptions<T> options, CancellationToken cToken)
        {
            throw new NotImplementedException();
        }

        Task<TResult[]> IPersistenceContext<IPersistentNoSql>.FindMany<T, TResult>(PersistenceSelectorOptions<T, TResult> options, CancellationToken cToken)
        {
            throw new NotImplementedException();
        }

        Task<T?> IPersistenceContext<IPersistentNoSql>.FindSingle<T>(PersistenceQueryOptions<T> options, CancellationToken cToken) where T : class
        {
            throw new NotImplementedException();
        }

        IQueryable<T> IPersistenceContext<IPersistentNoSql>.GetQuery<T>()
        {
            throw new NotImplementedException();
        }

        Task<bool> IPersistenceContext<IPersistentNoSql>.IsExists<T>(PersistenceQueryOptions<T> options, CancellationToken cToken)
        {
            throw new NotImplementedException();
        }

        Task<T[]> IPersistenceContext<IPersistentNoSql>.Update<T>(PersistenceUpdateOptions<T> options, CancellationToken cToken)
        {
            throw new NotImplementedException();
        }
    }
}
