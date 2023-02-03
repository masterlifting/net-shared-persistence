using System.Linq.Expressions;

using Shared.Persistence.Abstractions.Entities;

using static Shared.Persistence.Abstractions.Constants.Enums;

namespace Shared.Persistence.Abstractions.Contexts
{
    public interface IMongoPersistenceContext : IPersistenceContext<IPersistentNoSql>
    {
        Task<T[]> UpdateAsync<T>(Expression<Func<T, bool>> condition, Dictionary<ContextCommand, (string Name, object Value)> updater, CancellationToken cToken = default) where T : class, IPersistentNoSql;
    }
}
