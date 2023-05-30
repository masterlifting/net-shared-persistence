using System.Linq.Expressions;

namespace Net.Shared.Persistence.Models.Contexts
{
    public sealed class PersistenceQueryOptions<T> where T : class
    {
        public Expression<Func<T, bool>> Filter { get; set; } = _ => true;
        public Expression<Func<T, object>>? OrderBy { get; set; }
        public int? Take { get; set; }
        public int? Skip { get; set; }

        public void BuildQuery(ref IQueryable<T> query)
        {
            query = query.Where(Filter);

            if (OrderBy != null)
                query = query.OrderBy(OrderBy);

            if (Skip.HasValue)
                query = query.Skip(Skip.Value);

            if (Take.HasValue)
                query = query.Take(Take.Value);
        }
    }
    public sealed class PersistenceQuerySelectOptions<T, TResult> where T : class
    {
        public PersistenceQueryOptions<T> QueryOptions { get; set; } = new();
        public Func<T, TResult> Selector { get; set; } = null!;
    }
}
