﻿using System.Linq.Expressions;

using Net.Shared.Persistence.Abstractions.Interfaces.Contexts;
using Net.Shared.Persistence.Abstractions.Interfaces.Entities;
using Net.Shared.Persistence.Abstractions.Interfaces.Entities.Catalogs;
using Net.Shared.Persistence.Abstractions.Interfaces.Repositories;
using Net.Shared.Persistence.Abstractions.Interfaces.Repositories.NoSql;
using Net.Shared.Persistence.Abstractions.Models.Contexts;
using Net.Shared.Persistence.Contexts;

using static Net.Shared.Persistence.Abstractions.Constants.Enums;

namespace Net.Shared.Persistence.Repositories.MongoDb;

public sealed class MongoDbProcessRepository(MongoDbContext context) : IPersistenceNoSqlProcessRepository
{
    private readonly MongoDbContext _context = context;
    public IPersistenceNoSqlContext Context { get; } = context;

    public Task<T[]> GetProcessSteps<T>(CancellationToken cToken = default) where T : class, IPersistentNoSql, IPersistentProcessStep =>
        _context.FindMany<T>(new(), cToken);
    Task<T[]> IPersistenceProcessRepository<IPersistentNoSql>.GetProcessSteps<T>(Expression<Func<T, bool>> filter, CancellationToken cToken)
    {
        throw new NotImplementedException();
    }
    public async Task<T[]> GetProcessableData<T>(Guid hostId, IPersistentProcessStep step, int limit, CancellationToken cToken = default) where T : class, IPersistentNoSql, IPersistentProcess
    {
        var updated = DateTime.UtcNow;

        var options = new PersistenceUpdateOptions<T>(Update)
        {
            QueryOptions = new()
            {
                Filter = x =>
                    x.HostId == null || x.HostId == hostId
                    && x.StepId == step.Id
                    && x.StatusId == (int)ProcessStatuses.Ready,
                Take = limit,
                OrderBy = x => x.Updated
            }
        };

        return await _context.Update(options, cToken);

        void Update(T x)
        {
            x.HostId = hostId;
            x.StatusId = (int)ProcessStatuses.Processing;
            x.Attempt++;
            x.Updated = updated;
        }
    }
    Task<T[]> IPersistenceProcessRepository<IPersistentNoSql>.GetProcessableData<T>(Guid hostId, IPersistentProcessStep step, int limit, Expression<Func<T, bool>> filter, CancellationToken cToken)
    {
        throw new NotImplementedException();
    }
    public async Task<T[]> GetUnprocessedData<T>(Guid hostId, IPersistentProcessStep step, int limit, DateTime updateTime, int maxAttempts, CancellationToken cToken = default) where T : class, IPersistentNoSql, IPersistentProcess
    {
        var updated = DateTime.UtcNow;

        var options = new PersistenceUpdateOptions<T>(Update)
        {
            QueryOptions = new()
            {
                Filter = x =>
                    x.HostId == hostId
                    && x.StepId == step.Id
                    && ((x.StatusId == (int)ProcessStatuses.Processing && x.Updated < updateTime) || x.StatusId == (int)ProcessStatuses.Error)
                    && x.Attempt < maxAttempts,
                OrderBy = x => x.Updated,
                Take = limit
            }
        };

        return await _context.Update(options, cToken);

        void Update(T x)
        {
            x.StatusId = (int)ProcessStatuses.Processing;
            x.Attempt++;
            x.Updated = updated;
        }
    }
    Task<T[]> IPersistenceProcessRepository<IPersistentNoSql>.GetUnprocessedData<T>(Guid hostId, IPersistentProcessStep step, int limit, DateTime updateTime, int maxAttempts, Expression<Func<T, bool>> filter, CancellationToken cToken)
    {
        throw new NotImplementedException();
    }
    public async Task SetProcessedData<T>(Guid hostId, IPersistentProcessStep currentStep, IPersistentProcessStep? nextStep, IEnumerable<T> data, CancellationToken cToken = default) where T : class, IPersistentNoSql, IPersistentProcess
    {
        var updated = DateTime.UtcNow;

        var options = new PersistenceUpdateOptions<T>(Update, data)
        {
            QueryOptions = new()
            {
                Filter = x =>
                x.HostId == hostId
                && x.StepId == currentStep.Id
                && x.StatusId == (int)ProcessStatuses.Processing
            }
        };

        await _context.Update(options, cToken);
        
        void Update(T x)
        {
            x.Updated = updated;

            if (x.StatusId != (int)ProcessStatuses.Error)
            {
                x.Error = null;

                switch (nextStep)
                {
                    case not null:
                        x.StatusId = (int)ProcessStatuses.Ready;
                        x.StepId = nextStep.Id;
                        break;
                    default:
                        x.StatusId = (int)ProcessStatuses.Completed;
                        break;
                }
            }
        }
    }
}
