namespace Net.Shared.Persistence.Abstractions.Entities;

public interface IPersistentNoSql : IPersistent
{
    string DocumentVersion { get; set; }
}
