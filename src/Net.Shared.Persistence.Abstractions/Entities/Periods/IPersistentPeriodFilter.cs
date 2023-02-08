using Net.Shared.Persistence.Abstractions.Entities;

using System.Linq.Expressions;

namespace Net.Shared.Persistence.Abstractions.Entities.Periods;

public interface IPersistentPeriodFilter<T> where T : class, IPersistentPeriod, IPersistent
{
    Expression<Func<T, bool>> Predicate { get; set; }
}