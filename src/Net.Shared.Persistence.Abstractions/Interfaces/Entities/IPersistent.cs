namespace Net.Shared.Persistence.Abstractions.Interfaces.Entities;

public interface IPersistent
{
    DateTime Created { get; set; }
    public string? Description { get; set; }
}