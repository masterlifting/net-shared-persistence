using System.Linq.Expressions;

namespace Net.Shared.Persistence.Abstractions.Models.Contexts;

public sealed record PersistenceQueryOptions<T> where T : class
{
    public Expression<Func<T, bool>> Filter { get; set; } = _ => true;
    public Expression<Func<T, object>>? OrderBy { get; set; }
    public int? Take { get; set; }
    public int? Skip { get; set; }

    public void BuildQuery(ref IQueryable<T> query)
    {
        query = query.Where(Filter);

        if (OrderBy is not null)
            query = query.OrderBy(OrderBy);

        if (Skip.HasValue)
            query = query.Skip(Skip.Value);

        if (Take.HasValue)
            query = query.Take(Take.Value);
    }
}
