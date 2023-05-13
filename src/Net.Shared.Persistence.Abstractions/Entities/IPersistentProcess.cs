namespace Net.Shared.Persistence.Abstractions.Entities;

public interface IPersistentProcess : IPersistent
{
    Guid HostId { get; set; }
    int StatusId { get; set; }
    int StepId { get; set; }
    byte Attempt { get; set; }
    string? Error { get; set; }
    DateTime Updated { get; set; }
}