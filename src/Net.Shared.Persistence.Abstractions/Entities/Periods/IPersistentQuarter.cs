namespace Shared.Persistence.Abstractions.Entities.Periods;

public interface IPersistentQuarter : IPersistentPeriod
{
    int Year { get; set; }
    byte Quarter { get; set; }
}