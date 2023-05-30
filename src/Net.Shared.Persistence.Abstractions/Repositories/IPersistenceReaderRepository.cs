﻿using Net.Shared.Persistence.Abstractions.Entities;
using Net.Shared.Persistence.Abstractions.Entities.Catalogs;
using Net.Shared.Persistence.Models.Contexts;

namespace Net.Shared.Persistence.Abstractions.Repositories;

public interface IPersistenceReaderRepository<TEntity> where TEntity : class, IPersistent
{
    Task<bool> IsExists<T>(PersistenceQueryOptions<T> options, CancellationToken cToken = default) where T : class, TEntity;
    Task<T?> FindSingle<T>(PersistenceQueryOptions<T> options, CancellationToken cToken = default) where T : class, TEntity;
    Task<T?> FindFirst<T>(PersistenceQueryOptions<T> options, CancellationToken cToken = default) where T : class, TEntity;
    Task<T[]> FindMany<T>(PersistenceQueryOptions<T> options, CancellationToken cToken = default) where T : class, TEntity;
    Task<TResult[]> FindMany<T, TResult>(PersistenceSelectorOptions<T, TResult> options, CancellationToken cToken = default) where T : class, TEntity;

    #region Catalogs API
    Task<T[]> GetCatalogs<T>(CancellationToken cToken = default) where T : class, TEntity, IPersistentCatalog;
    
    Task<T> GetCatalogById<T>(int id, CancellationToken cToken = default) where T : class, TEntity, IPersistentCatalog;
    Task<Dictionary<int, T>> GetCatalogsDictionaryById<T>(CancellationToken cToken = default) where T : class, TEntity, IPersistentCatalog;
    
    Task<T> GetCatalogByName<T>(string name, CancellationToken cToken = default) where T : class, TEntity, IPersistentCatalog;
    Task<Dictionary<string, T>> GetCatalogsDictionaryByName<T>(CancellationToken cToken = default) where T : class, TEntity, IPersistentCatalog;
    
    Task<T> GetCatalogByEnum<T, TEnum>(TEnum value, CancellationToken cToken = default) where T : class, TEntity, IPersistentCatalog where TEnum : Enum;
    Task<Dictionary<TEnum, T>> GetCatalogsDictionaryByEnum<T, TEnum>(CancellationToken cToken = default) where T : class, TEntity, IPersistentCatalog where TEnum : Enum;
    #endregion
}
