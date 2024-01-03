using Azure.Data.Tables;

using Net.Shared.Abstractions.Models.Data;
using Net.Shared.Persistence.Abstractions.Interfaces.Repositories;
using Net.Shared.Persistence.Abstractions.Models.Contexts;
using Net.Shared.Persistence.Contexts;

namespace Net.Shared.Persistence.Repositories.AzureTable;

public sealed class AzureTableWriterRepository : IPersistenceWriterRepository<ITableEntity>
{
    private readonly AzureTableContext _context;

    public AzureTableWriterRepository(AzureTableContext context)
    {
        _context = context;
    }
    Task IPersistenceWriterRepository<ITableEntity>.CreateMany<T>(IReadOnlyCollection<T> entities, CancellationToken cToken)
    {
        throw new NotImplementedException();
    }

    Task IPersistenceWriterRepository<ITableEntity>.CreateOne<T>(T entity, CancellationToken cToken)
    {
        return _context.CreateOne(entity, cToken);
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
        return _context.Update(options, cToken);
    }
}
