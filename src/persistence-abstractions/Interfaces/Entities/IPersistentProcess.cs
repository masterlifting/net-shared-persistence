namespace Net.Shared.Persistence.Abstractions.Interfaces.Entities;

public interface IPersistentProcess : IPersistent
{
    Guid? HostId { get; set; }
    int StatusId { get; set; }
    int StepId { get; set; }
    int Attempt { get; set; }
    string? Error { get; set; }
    DateTime Updated { get; set; }
}
