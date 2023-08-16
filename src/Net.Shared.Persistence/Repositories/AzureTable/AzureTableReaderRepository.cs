﻿using Azure.Data.Tables;

using Net.Shared.Persistence.Abstractions.Repositories;
using Net.Shared.Persistence.Models.Contexts;

namespace Net.Shared.Persistence.Repositories.AzureTable;

public sealed class AzureTableReaderRepository : IPersistenceReaderRepository<ITableEntity>
{
    Task<T?> IPersistenceReaderRepository<ITableEntity>.FindFirst<T>(PersistenceQueryOptions<T> options, CancellationToken cToken) where T : class
    {
        throw new NotImplementedException();
    }

    Task<T[]> IPersistenceReaderRepository<ITableEntity>.FindMany<T>(PersistenceQueryOptions<T> options, CancellationToken cToken)
    {
        throw new NotImplementedException();
    }

    Task<TResult[]> IPersistenceReaderRepository<ITableEntity>.FindMany<T, TResult>(PersistenceSelectorOptions<T, TResult> options, CancellationToken cToken)
    {
        throw new NotImplementedException();
    }

    Task<T?> IPersistenceReaderRepository<ITableEntity>.FindSingle<T>(PersistenceQueryOptions<T> options, CancellationToken cToken) where T : class
    {
        throw new NotImplementedException();
    }

    Task<T> IPersistenceReaderRepository<ITableEntity>.GetCatalogByEnum<T, TEnum>(TEnum value, CancellationToken cToken)
    {
        throw new NotImplementedException();
    }

    Task<T> IPersistenceReaderRepository<ITableEntity>.GetCatalogById<T>(int id, CancellationToken cToken)
    {
        throw new NotImplementedException();
    }

    Task<T> IPersistenceReaderRepository<ITableEntity>.GetCatalogByName<T>(string name, CancellationToken cToken)
    {
        throw new NotImplementedException();
    }

    Task<T[]> IPersistenceReaderRepository<ITableEntity>.GetCatalogs<T>(CancellationToken cToken)
    {
        throw new NotImplementedException();
    }

    Task<Dictionary<TEnum, T>> IPersistenceReaderRepository<ITableEntity>.GetCatalogsDictionaryByEnum<T, TEnum>(CancellationToken cToken)
    {
        throw new NotImplementedException();
    }

    Task<Dictionary<int, T>> IPersistenceReaderRepository<ITableEntity>.GetCatalogsDictionaryById<T>(CancellationToken cToken)
    {
        throw new NotImplementedException();
    }

    Task<Dictionary<string, T>> IPersistenceReaderRepository<ITableEntity>.GetCatalogsDictionaryByName<T>(CancellationToken cToken)
    {
        throw new NotImplementedException();
    }

    Task<bool> IPersistenceReaderRepository<ITableEntity>.IsExists<T>(PersistenceQueryOptions<T> options, CancellationToken cToken)
    {
        throw new NotImplementedException();
    }
}
