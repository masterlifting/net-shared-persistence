namespace Shared.Persistence.Abstractions.Entities;

public interface IPersistentProcess : IPersistent
{
    Guid Id { get; init; }
    int ProcessStatusId { get; set; }
    int ProcessStepId { get; set; }
    byte ProcessAttempt { get; set; }
    string? Error { get; set; }
    DateTime Updated { get; set; }
}