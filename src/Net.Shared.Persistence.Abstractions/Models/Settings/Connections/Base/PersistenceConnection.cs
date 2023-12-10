using Net.Shared.Abstractions.Models.Settings;

namespace Net.Shared.Persistence.Abstractions.Models.Settings.Connections.Base;

public abstract record PersistenceConnection : Connection
{
    public string Database { get; set; } = null!;
    public abstract string ConnectionString { get; }
}
