using System.Linq.Expressions;

namespace Net.Shared.Persistence.Models.Contexts
{
    public sealed class PersistenceQueryOptions<T> where T : class
    {
        public Expression<Func<T, bool>> Filter { get; set; } = _ => true;
        public Expression<Func<T, object>>? OrderBy { get; set; }
        public Expression<Func<T, object>>? Selector { get; set; }
        public int? Take { get; set; }
        public int? Skip { get; set; }

        public void BuildQuery<TQueryable>(ref TQueryable query) where TQueryable : class, IQueryable<T>
        {
            query = (TQueryable)query.Where(Filter);

            if (OrderBy != null)
                query = (TQueryable)query.OrderBy(OrderBy);

            if (Skip.HasValue)
                query = (TQueryable)query.Skip(Skip.Value);

            if (Take.HasValue)
                query = (TQueryable)query.Take(Take.Value);

            if (Selector != null)
                query = (TQueryable)query.Select(Selector);
        }
    }
}
