﻿using Microsoft.EntityFrameworkCore;

using Net.Shared.Persistence.Abstractions.Interfaces.Entities;
using Net.Shared.Persistence.Abstractions.Interfaces.Entities.Catalogs;
using Net.Shared.Persistence.Abstractions.Interfaces.Repositories;
using Net.Shared.Persistence.Abstractions.Models.Contexts;
using Net.Shared.Persistence.Contexts;

namespace Net.Shared.Persistence.Repositories.PostgreSql;

public class PostgreSqlReaderRepository<TContext, TEntity>(TContext context) : IPersistenceReaderRepository<TEntity>
    where TContext : PostgreSqlContext
    where TEntity : IPersistentSql
{
    private readonly PostgreSqlContext _context = context;

    public Task<bool> IsExists<T>(PersistenceQueryOptions<T> options, CancellationToken cToken = default) where T : class, TEntity =>
        _context.IsExists(options, cToken);

    public Task<T?> FindSingle<T>(PersistenceQueryOptions<T> options, CancellationToken cToken = default) where T : class, TEntity =>
        _context.FindSingle(options, cToken);
    public Task<T?> FindFirst<T>(PersistenceQueryOptions<T> options, CancellationToken cToken = default) where T : class, TEntity =>
        _context.FindFirst(options, cToken);

    public Task<T[]> FindMany<T>(PersistenceQueryOptions<T> options, CancellationToken cToken = default) where T : class, TEntity =>
        _context.FindMany(options, cToken);
    public Task<TResult[]> FindMany<T, TResult>(PersistenceSelectorOptions<T, TResult> options, CancellationToken cToken = default) where T : class, TEntity =>
        _context.FindMany(options, cToken);

    #region Specialized API
    public Task<T?> FindById<T>(object[] id, CancellationToken cToken) where T : class, IPersistentSql =>
        _context.FindById<T>(id, cToken);
    public Task<T?> FindById<T>(object id, CancellationToken cToken) where T : class, IPersistentSql =>
        _context.FindById<T>(id, cToken);
    #endregion
    
    #region Catalogs API
    public Task<T[]> GetCatalogs<T>(CancellationToken cToken = default) where T : class, IPersistentCatalog, TEntity =>
        _context.FindMany<T>(new(), cToken);

    public async Task<T> GetCatalogById<T>(int id, CancellationToken cToken = default) where T : class, IPersistentCatalog, TEntity =>
        await _context.FindSingle<T>(new() { Filter = x => x.Id == id }, cToken)
        ?? throw new InvalidOperationException($"Catalog {typeof(T).Name} with id {id} not found");
    public Task<Dictionary<int, T>> GetCatalogsDictionaryById<T>(CancellationToken cToken = default) where T : class, IPersistentCatalog, TEntity =>
            _context.GetQuery<T>().ToDictionaryAsync(x => x.Id, cToken);

    public async Task<T> GetCatalogByName<T>(string name, CancellationToken cToken = default) where T : class, IPersistentCatalog, TEntity =>
        await _context.FindSingle<T>(new() { Filter = x => x.Name.Equals(name) }, cToken)
        ?? throw new InvalidOperationException($"Catalog {typeof(T).Name} with name {name} not found");
    public Task<Dictionary<string, T>> GetCatalogsDictionaryByName<T>(CancellationToken cToken = default) where T : class, IPersistentCatalog, TEntity =>
            _context.GetQuery<T>().ToDictionaryAsync(x => x.Name, cToken);

    public async Task<T> GetCatalogByEnum<T, TEnum>(TEnum value, CancellationToken cToken)
        where T : class, IPersistentCatalog, TEntity
        where TEnum : Enum
    {
        var name = Enum.GetName(typeof(TEnum), value);

        return name is null
            ? throw new InvalidOperationException($"Enum {typeof(TEnum).Name} does not contain value {value}")
            : await GetCatalogByName<T>(name, cToken);
    }
    public Task<Dictionary<TEnum, T>> GetCatalogsDictionaryByEnum<T, TEnum>(CancellationToken cToken)
        where T : class, IPersistentCatalog, TEntity
        where TEnum : Enum =>
            _context.GetQuery<T>().ToDictionaryAsync(x => (TEnum)Enum.Parse(typeof(TEnum), x.Name.AsSpan()), cToken);

    #endregion
}
