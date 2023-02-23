namespace Net.Shared.Persistence.Models.Entities.Catalogs;

public abstract class PersistentCatalog
{
    public int Id { get; init; }
    public string Name { get; init; } = null!;
    public DateTime Created { get; init; } = DateTime.UtcNow;
    public string? Description { get; init; }
}