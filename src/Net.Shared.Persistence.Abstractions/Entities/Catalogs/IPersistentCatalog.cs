namespace Net.Shared.Persistence.Abstractions.Entities.Catalogs;

public interface IPersistentCatalog : IPersistent
{
    int Id { get; init; }
    string Name { get; set; }
}