namespace Shared.Persistence.Abstractions.Entities;

public interface IPersistent
{
    DateTime Created { get; init; }
    public string? Description { get; init; }
}