namespace Shared.Persistence.Abstractions.Entities
{
    public interface IPersistentNoSql : IPersistent
    {
        string JsonVersion { get; init; }
    }
}
