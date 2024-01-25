﻿using System.Linq.Expressions;

using Net.Shared.Persistence.Abstractions.Interfaces.Entities;
using Net.Shared.Persistence.Abstractions.Interfaces.Entities.Catalogs;

namespace Net.Shared.Persistence.Abstractions.Interfaces.Repositories;

public interface IPersistenceProcessRepository<TEntity>
{
    Task<T[]> GetProcessSteps<T>(CancellationToken cToken = default) where T : class, TEntity, IPersistentProcessStep;
    Task<T[]> GetProcessSteps<T>(Expression<Func<T, bool>> filter, CancellationToken cToken = default) where T : class, TEntity, IPersistentProcessStep;
    Task<T[]> GetProcessableData<T>(Guid correlationId, IPersistentProcessStep step, int limit, CancellationToken cToken = default) where T : class, TEntity, IPersistentProcess;
    Task<T[]> GetProcessableData<T>(Guid correlationId, IPersistentProcessStep step, int limit, Expression<Func<T, bool>> filter, CancellationToken cToken = default) where T : class, TEntity, IPersistentProcess;
    Task<T[]> GetUnprocessedData<T>(Guid correlationId, IPersistentProcessStep step, int limit, DateTime updateTime, int maxAttempts, CancellationToken cToken = default) where T : class, TEntity, IPersistentProcess;
    Task<T[]> GetUnprocessedData<T>(Guid correlationId, IPersistentProcessStep step, int limit, DateTime updateTime, int maxAttempts, Expression<Func<T, bool>> filter, CancellationToken cToken = default) where T : class, TEntity, IPersistentProcess;
    Task SetProcessedData<T>(Guid correlationId, IPersistentProcessStep currentStep, IPersistentProcessStep? nextStep, IEnumerable<T> data, CancellationToken cToken = default) where T : class, TEntity, IPersistentProcess;
}
