namespace Shared.Persistence.Abstractions.Entities.Periods;

public interface IPersistentDate : IPersistentPeriod
{
    DateOnly Date { get; set; }
}