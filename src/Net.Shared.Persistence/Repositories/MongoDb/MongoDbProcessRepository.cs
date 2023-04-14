using System.Linq.Expressions;
using Net.Shared.Persistence.Abstractions.Entities.Catalogs;
using Net.Shared.Persistence.Abstractions.Repositories;
using Net.Shared.Persistence.Contexts;
using static Net.Shared.Persistence.Models.Constants.Enums;

namespace Net.Shared.Persistence.Repositories.MongoDb;

public sealed class MongoDbProcessRepository : IPersistenceProcessRepository
{
    private readonly MongoDbContext _context;
    public MongoDbProcessRepository(MongoDbContext context) => _context = context;

    Task<T[]> IPersistenceProcessRepository.GetProcessableData<T>(IPersistentProcessStep step, int limit, CancellationToken cToken)
    {
        throw new NotImplementedException();
    }

    Task<T[]> IPersistenceProcessRepository.GetSteps<T>(CancellationToken cToken)
    {
        throw new NotImplementedException();
    }

    Task<T[]> IPersistenceProcessRepository.GetUnprocessableData<T>(IPersistentProcessStep step, int limit, DateTime updateTime, int maxAttempts, CancellationToken cToken)
    {
        throw new NotImplementedException();
    }

    Task IPersistenceProcessRepository.SetProcessableData<T>(IPersistentProcessStep? step, IEnumerable<T> entities, CancellationToken cToken)
    {
        throw new NotImplementedException();
    }

    // Task<Dictionary<string, T>> IPersistenceProcessRepository.GetSteps<T>(CancellationToken cToken) =>
    //     Task.Run(() => _context.SetEntity<T>().ToDictionary(x => x.Name));
    // async Task<T[]> IPersistenceProcessRepository.GetProcessableData<T>(IPersistentProcessStep step, int limit, CancellationToken cToken)
    // {
    //     Expression<Func<T, bool>> condition = x =>
    //         x.ProcessStepId == step.Id
    //         && x.ProcessStatusId == (int)ProcessStatuses.Ready;

    //     var updater = (T x) =>
    //     {
    //         x.Updated = DateTime.UtcNow;
    //         x.ProcessStatusId = (int)ProcessStatuses.Processing;
    //         x.ProcessAttempt++;
    //     };

    //     return await _context.Update(condition, updater, cToken);
    // }
    // async Task<T[]> IPersistenceProcessRepository.GetUnprocessableData<T>(IPersistentProcessStep step, int limit, DateTime updateTime, int maxAttempts, CancellationToken cToken)
    // {
    //     Expression<Func<T, bool>> condition = x =>
    //         x.ProcessStepId == step.Id
    //         && ((x.ProcessStatusId == (int)ProcessStatuses.Processing && x.Updated < updateTime) || x.ProcessStatusId == (int)ProcessStatuses.Error)
    //         && x.ProcessAttempt < maxAttempts;

    //     var updater = (T x) =>
    //     {
    //         x.Updated = DateTime.UtcNow;
    //         x.ProcessStatusId = (int)ProcessStatuses.Processing;
    //         x.ProcessAttempt++;
    //     };

    //     return await _context.Update(condition, updater, cToken);
    // }
    // async Task IPersistenceProcessRepository.SetProcessableData<T>(IPersistentProcessStep? step, IEnumerable<T> entities, CancellationToken cToken)
    // {
    //     try
    //     {
    //         await _context.StartTransaction(cToken);

    //         var count = 0;
    //         foreach (var entity in entities)
    //         {
    //             entity.Updated = DateTime.UtcNow;

    //             if (entity.ProcessStatusId != (int)ProcessStatuses.Error)
    //             {
    //                 entity.Error = null;

    //                 if (step is not null)
    //                     entity.ProcessStepId = step.Id;
    //             }

    //             await _context.Update(x => x.Id == entity.Id, entity, cToken);

    //             count++;
    //         }

    //         await _context.CommitTransaction(cToken);

    //         _logger.LogTrace(_repositoryInfo, Constants.Actions.Updated, Constants.Actions.Success, count);
    //     }
    //     catch (Exception exception)
    //     {
    //         await _context.RollbackTransaction(cToken);

    //         throw new NetSharedPersistenceException(exception);
    //     }
    // }
}
