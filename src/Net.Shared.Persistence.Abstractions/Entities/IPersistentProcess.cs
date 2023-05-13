namespace Net.Shared.Persistence.Abstractions.Entities;

public interface IPersistentProcess : IPersistent
{
    Guid ProcessHostId { get; set; }
    int ProcessStatusId { get; set; }
    int ProcessStepId { get; set; }
    byte ProcessAttempt { get; set; }
    string? Error { get; set; }
    DateTime Updated { get; set; }
}