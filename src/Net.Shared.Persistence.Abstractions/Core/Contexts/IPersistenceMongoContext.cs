using System.Linq.Expressions;

using Net.Shared.Persistence.Abstractions.Entities;

using static Net.Shared.Persistence.Models.Constants.Enums;

namespace Net.Shared.Persistence.Abstractions.Core.Contexts
{
    public interface IPersistenceMongoContext : IPersistenceContext<IPersistentNoSql>
    {
        Task<T[]> Update<T>(Expression<Func<T, bool>> filter, Dictionary<ContextCommands, (string Name, object Value)> updater, CancellationToken cToken = default) where T : class, IPersistentNoSql;
    }
}
