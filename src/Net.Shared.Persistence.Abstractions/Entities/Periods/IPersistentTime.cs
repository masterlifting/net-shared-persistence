namespace Shared.Persistence.Abstractions.Entities.Periods;

public interface IPersistentTime : IPersistentPeriod
{
    TimeOnly Time { get; set; }
}