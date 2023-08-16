using Net.Shared.Persistence.Abstractions.Contexts;
using Net.Shared.Persistence.Abstractions.Entities;
using Net.Shared.Persistence.Abstractions.Repositories;
using Net.Shared.Persistence.Abstractions.Repositories.NoSql;
using Net.Shared.Persistence.Models.Contexts;

namespace Net.Shared.Persistence.Repositories.AzureTable;

public sealed class AzureTableReaderRepository : IPersistenceNoSqlReaderRepository
{
    public IPersistenceNoSqlContext Context { get; }

    Task<T?> IPersistenceReaderRepository<IPersistentNoSql>.FindFirst<T>(PersistenceQueryOptions<T> options, CancellationToken cToken) where T : class
    {
        throw new NotImplementedException();
    }

    Task<T[]> IPersistenceReaderRepository<IPersistentNoSql>.FindMany<T>(PersistenceQueryOptions<T> options, CancellationToken cToken)
    {
        throw new NotImplementedException();
    }

    Task<TResult[]> IPersistenceReaderRepository<IPersistentNoSql>.FindMany<T, TResult>(PersistenceSelectorOptions<T, TResult> options, CancellationToken cToken)
    {
        throw new NotImplementedException();
    }

    Task<T?> IPersistenceReaderRepository<IPersistentNoSql>.FindSingle<T>(PersistenceQueryOptions<T> options, CancellationToken cToken) where T : class
    {
        throw new NotImplementedException();
    }

    Task<T> IPersistenceReaderRepository<IPersistentNoSql>.GetCatalogByEnum<T, TEnum>(TEnum value, CancellationToken cToken)
    {
        throw new NotImplementedException();
    }

    Task<T> IPersistenceReaderRepository<IPersistentNoSql>.GetCatalogById<T>(int id, CancellationToken cToken)
    {
        throw new NotImplementedException();
    }

    Task<T> IPersistenceReaderRepository<IPersistentNoSql>.GetCatalogByName<T>(string name, CancellationToken cToken)
    {
        throw new NotImplementedException();
    }

    Task<T[]> IPersistenceReaderRepository<IPersistentNoSql>.GetCatalogs<T>(CancellationToken cToken)
    {
        throw new NotImplementedException();
    }

    Task<Dictionary<TEnum, T>> IPersistenceReaderRepository<IPersistentNoSql>.GetCatalogsDictionaryByEnum<T, TEnum>(CancellationToken cToken)
    {
        throw new NotImplementedException();
    }

    Task<Dictionary<int, T>> IPersistenceReaderRepository<IPersistentNoSql>.GetCatalogsDictionaryById<T>(CancellationToken cToken)
    {
        throw new NotImplementedException();
    }

    Task<Dictionary<string, T>> IPersistenceReaderRepository<IPersistentNoSql>.GetCatalogsDictionaryByName<T>(CancellationToken cToken)
    {
        throw new NotImplementedException();
    }

    Task<bool> IPersistenceReaderRepository<IPersistentNoSql>.IsExists<T>(PersistenceQueryOptions<T> options, CancellationToken cToken)
    {
        throw new NotImplementedException();
    }
}
