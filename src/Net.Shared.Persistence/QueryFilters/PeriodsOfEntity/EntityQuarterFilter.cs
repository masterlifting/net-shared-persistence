using Shared.Persistence.Abstractions.Entities;
using Shared.Persistence.Abstractions.Entities.Periods;

using System.Linq.Expressions;

using static Shared.Persistence.Constants.Enums;

namespace Shared.Persistence.QueryFilters.PeriodsOfEntity;

public sealed class EntityQuarterFilter<T> : IPersistentPeriodFilter<T> where T : class, IPersistentQuarter, IPersistent
{
    public int Year { get; }
    public byte Quarter { get; }
    public Expression<Func<T, bool>> Predicate { get; set; }

    public EntityQuarterFilter(Comparisons comparisons, int year)
    {
        Year = year;
        Quarter = 1;

        Predicate = comparisons switch
        {
            Comparisons.Equal => x => x.Year == Year,
            Comparisons.More => x => x.Year >= Year,
            Comparisons.Less => x => x.Year <= Year,
            _ => throw new ArgumentOutOfRangeException(nameof(comparisons), comparisons, null)
        };
    }
    public EntityQuarterFilter(Comparisons comparisons, int year, byte quarter)
    {
        Year = year;
        Quarter = quarter;

        Predicate = comparisons switch
        {
            Comparisons.Equal => x => x.Year == Year && x.Quarter == Quarter,
            Comparisons.More => x => x.Year > Year || x.Year == Year && x.Quarter >= Quarter,
            Comparisons.Less => x => x.Year < Year || x.Year == Year && x.Quarter <= Quarter,
            _ => throw new ArgumentOutOfRangeException(nameof(comparisons), comparisons, null)
        };
    }
}