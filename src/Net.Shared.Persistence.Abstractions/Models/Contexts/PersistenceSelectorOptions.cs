using System.Linq.Expressions;

namespace Net.Shared.Persistence.Abstractions.Models.Contexts;

public sealed record PersistenceSelectorOptions<TData, TResult> where TData : class
{
    public PersistenceQueryOptions<TData> QueryOptions { get; set; } = new();
    public Expression<Func<TData, TResult>> Selector { get; set; } = null!;
}
