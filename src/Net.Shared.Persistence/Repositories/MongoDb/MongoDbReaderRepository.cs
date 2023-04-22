﻿using System.Linq.Expressions;

using Net.Shared.Persistence.Abstractions.Contexts;
using Net.Shared.Persistence.Abstractions.Entities;
using Net.Shared.Persistence.Abstractions.Entities.Catalogs;
using Net.Shared.Persistence.Abstractions.Repositories.NoSql;
using Net.Shared.Persistence.Contexts;

namespace Net.Shared.Persistence.Repositories.MongoDb;

public sealed class MongoDbReaderRepository : IPersistenceNoSqlReaderRepository
{
    public MongoDbReaderRepository(MongoDbContext context)
    {
        _context = context;
        Context = context;
    }

    #region PRIVATE FIELDS
    private readonly MongoDbContext _context;
    #endregion

    #region PUBLIC PROPERTIES
    public IPersistenceNoSqlContext Context { get; }
    #endregion

    #region PUBLIC METHODS
    public Task<T?> FindSingle<T>(Expression<Func<T, bool>> filter, CancellationToken cToken) where T : class, IPersistentNoSql =>
        _context.FindSingle(filter, cToken);
    public Task<T?> FindFirst<T>(Expression<Func<T, bool>> filter, CancellationToken cToken) where T : class, IPersistentNoSql =>
        _context.FindFirst(filter, cToken);
    public Task<T[]> FindMany<T>(Expression<Func<T, bool>> filter, CancellationToken cToken) where T : class, IPersistentNoSql =>
        _context.FindMany(filter, cToken);

    public Task<T[]> GetCatalogs<T>(CancellationToken cToken) where T : class, IPersistentCatalog, IPersistentNoSql =>
        _context.FindMany<T>(_ => true, cToken);
    public Task<T?> GetCatalogById<T>(int id, CancellationToken cToken) where T : class, IPersistentCatalog, IPersistentNoSql =>
        _context.FindSingle<T>(x => x.Id == id, cToken);
    public Task<T?> GetCatalogByName<T>(string name, CancellationToken cToken) where T : class, IPersistentCatalog, IPersistentNoSql =>
        _context.FindSingle<T>(x => x.Name.Equals(name), cToken);
    public Task<Dictionary<int, T>> GetCatalogsDictionaryById<T>(CancellationToken cToken) where T : class, IPersistentCatalog, IPersistentNoSql =>
            Task.Run(() => _context.SetEntity<T>().ToDictionary(x => x.Id));
    public Task<Dictionary<string, T>> GetCatalogsDictionaryByName<T>(CancellationToken cToken) where T : class, IPersistentCatalog, IPersistentNoSql =>
            Task.Run(() => _context.SetEntity<T>().ToDictionary(x => x.Name));
    #endregion
}
