using Net.Shared.Models.Domain;
using Net.Shared.Persistence.Abstractions.Contexts;
using Net.Shared.Persistence.Abstractions.Entities;
using Net.Shared.Persistence.Abstractions.Repositories;
using Net.Shared.Persistence.Abstractions.Repositories.NoSql;
using Net.Shared.Persistence.Models.Contexts;

namespace Net.Shared.Persistence.Repositories.AzureTable;

public sealed class AzureTableWriterRepository : IPersistenceNoSqlWriterRepository
{
    public IPersistenceNoSqlContext Context { get; }

    Task IPersistenceWriterRepository<IPersistentNoSql>.CreateMany<T>(IReadOnlyCollection<T> entities, CancellationToken cToken)
    {
        throw new NotImplementedException();
    }

    Task IPersistenceWriterRepository<IPersistentNoSql>.CreateOne<T>(T entity, CancellationToken cToken)
    {
        throw new NotImplementedException();
    }

    Task<long> IPersistenceWriterRepository<IPersistentNoSql>.Delete<T>(PersistenceQueryOptions<T> options, CancellationToken cToken)
    {
        throw new NotImplementedException();
    }

    Task<Result<T>> IPersistenceWriterRepository<IPersistentNoSql>.TryCreateMany<T>(IReadOnlyCollection<T> entities, CancellationToken cToken)
    {
        throw new NotImplementedException();
    }

    Task<Result<T>> IPersistenceWriterRepository<IPersistentNoSql>.TryCreateOne<T>(T entity, CancellationToken cToken)
    {
        throw new NotImplementedException();
    }

    Task<Result<long>> IPersistenceWriterRepository<IPersistentNoSql>.TryDelete<T>(PersistenceQueryOptions<T> options, CancellationToken cToken)
    {
        throw new NotImplementedException();
    }

    Task<Result<T>> IPersistenceWriterRepository<IPersistentNoSql>.TryUpdate<T>(PersistenceUpdateOptions<T> options, CancellationToken cToken)
    {
        throw new NotImplementedException();
    }

    Task<T[]> IPersistenceWriterRepository<IPersistentNoSql>.Update<T>(PersistenceUpdateOptions<T> options, CancellationToken cToken)
    {
        throw new NotImplementedException();
    }
}
