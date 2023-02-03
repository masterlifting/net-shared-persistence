namespace Shared.Persistence.Abstractions.Entities;

public interface IPersistentCatalog : IPersistent
{
    int Id { get; init; }
    string Name { get; init; }
}