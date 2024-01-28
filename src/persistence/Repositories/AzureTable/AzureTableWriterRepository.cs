using Azure.Data.Tables;

using Microsoft.Extensions.Logging;

using Net.Shared.Abstractions.Models.Data;
using Net.Shared.Extensions.Logging;
using Net.Shared.Persistence.Abstractions.Interfaces.Entities;
using Net.Shared.Persistence.Abstractions.Interfaces.Repositories;
using Net.Shared.Persistence.Abstractions.Models.Contexts;
using Net.Shared.Persistence.Contexts;

namespace Net.Shared.Persistence.Repositories.AzureTable;

public class AzureTableWriterRepository<TEntity> : IPersistenceWriterRepository<TEntity> where TEntity : IPersistent, ITableEntity
{
    private readonly ILogger _log;
    private readonly AzureTableContext _context;

    private readonly string _repository;
    public AzureTableWriterRepository(ILogger<AzureTableWriterRepository<TEntity>> logger, AzureTableContext context)
    {
        _log = logger;
        _context = context;
        _repository = nameof(AzureTableWriterRepository<TEntity>) + ' ' + GetHashCode();

        _log.Warn(_repository);
    }

    public Task CreateOne<T>(T entity, CancellationToken cToken) where T : class, TEntity
    {
        return _context.CreateOne(entity, cToken);
    }
    public Task<Result<T>> TryCreateOne<T>(T entity, CancellationToken cToken) where T : class, TEntity
    {
        throw new NotImplementedException();
    }

    public Task CreateMany<T>(IReadOnlyCollection<T> entities, CancellationToken cToken) where T : class, TEntity
    {
        throw new NotImplementedException();
    }
    public Task<Result<T>> TryCreateMany<T>(IReadOnlyCollection<T> entities, CancellationToken cToken) where T : class, TEntity
    {
        throw new NotImplementedException();
    }

    public Task<T[]> Update<T>(PersistenceUpdateOptions<T> options, CancellationToken cToken) where T : class, TEntity
    {
        return _context.Update(options, cToken);
    }
    public Task<Result<T>> TryUpdate<T>(PersistenceUpdateOptions<T> options, CancellationToken cToken) where T : class, TEntity
    {
        throw new NotImplementedException();
    }

    public Task<long> Delete<T>(PersistenceQueryOptions<T> options, CancellationToken cToken) where T : class, TEntity
    {
        throw new NotImplementedException();
    }
    public Task<Result<long>> TryDelete<T>(PersistenceQueryOptions<T> options, CancellationToken cToken) where T : class, TEntity
    {
        throw new NotImplementedException();
    }
}
