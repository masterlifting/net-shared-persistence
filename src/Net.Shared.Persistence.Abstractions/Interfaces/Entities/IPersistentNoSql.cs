namespace Net.Shared.Persistence.Abstractions.Interfaces.Entities;

public interface IPersistentNoSql : IPersistent
{
    string DocumentVersion { get; set; }
}
