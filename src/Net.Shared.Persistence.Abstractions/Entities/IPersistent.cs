namespace Net.Shared.Persistence.Abstractions.Entities;

public interface IPersistent
{
    DateTime Created { get; set; }
    public string? Description { get; set; }
}