namespace Net.Shared.Persistence.Abstractions.Models.Entities.Catalogs;

public abstract class PersistentCatalog
{
    public int Id { get; init; }
    public string Name { get; set; } = null!;
    public DateTime Created { get; set; } = DateTime.UtcNow;
    public string? Description { get; set; }
}
