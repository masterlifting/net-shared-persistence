using System.Linq.Expressions;

namespace Shared.Persistence.Abstractions.Entities.Periods;

public interface IPersistentPeriodFilter<T> where T : class, IPersistentPeriod, IPersistent
{
    Expression<Func<T, bool>> Predicate { get; set; }
}