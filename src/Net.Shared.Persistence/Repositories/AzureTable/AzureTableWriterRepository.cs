using Azure.Data.Tables;

using Net.Shared.Models.Domain;
using Net.Shared.Persistence.Abstractions.Repositories;
using Net.Shared.Persistence.Models.Contexts;

namespace Net.Shared.Persistence.Repositories.AzureTable;

public sealed class AzureTableWriterRepository : IPersistenceWriterRepository<ITableEntity>
{
    Task IPersistenceWriterRepository<ITableEntity>.CreateMany<T>(IReadOnlyCollection<T> entities, CancellationToken cToken)
    {
        throw new NotImplementedException();
    }

    Task IPersistenceWriterRepository<ITableEntity>.CreateOne<T>(T entity, CancellationToken cToken)
    {
        throw new NotImplementedException();
    }

    Task<long> IPersistenceWriterRepository<ITableEntity>.Delete<T>(PersistenceQueryOptions<T> options, CancellationToken cToken)
    {
        throw new NotImplementedException();
    }

    Task<Result<T>> IPersistenceWriterRepository<ITableEntity>.TryCreateMany<T>(IReadOnlyCollection<T> entities, CancellationToken cToken)
    {
        throw new NotImplementedException();
    }

    Task<Result<T>> IPersistenceWriterRepository<ITableEntity>.TryCreateOne<T>(T entity, CancellationToken cToken)
    {
        throw new NotImplementedException();
    }

    Task<Result<long>> IPersistenceWriterRepository<ITableEntity>.TryDelete<T>(PersistenceQueryOptions<T> options, CancellationToken cToken)
    {
        throw new NotImplementedException();
    }

    Task<Result<T>> IPersistenceWriterRepository<ITableEntity>.TryUpdate<T>(PersistenceUpdateOptions<T> options, CancellationToken cToken)
    {
        throw new NotImplementedException();
    }

    Task<T[]> IPersistenceWriterRepository<ITableEntity>.Update<T>(PersistenceUpdateOptions<T> options, CancellationToken cToken)
    {
        throw new NotImplementedException();
    }
}
