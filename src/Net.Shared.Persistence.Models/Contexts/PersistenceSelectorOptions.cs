using System.Linq.Expressions;

namespace Net.Shared.Persistence.Models.Contexts;

public sealed class PersistenceSelectorOptions<TEntity, TResult> where TEntity : class
{
    public PersistenceQueryOptions<TEntity> QueryOptions { get; set; } = new();
    public Expression<Func<TEntity, TResult>> Selector { get; set; } = null!;
}
