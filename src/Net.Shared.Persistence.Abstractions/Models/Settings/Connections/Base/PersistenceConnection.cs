using System.ComponentModel.DataAnnotations;

using Net.Shared.Abstractions.Models.Settings;

namespace Net.Shared.Persistence.Abstractions.Models.Settings.Connections.Base;

public abstract record PersistenceConnection : Connection
{
    [Required]
    public string Database { get; set; } = null!;
    public abstract string ConnectionString { get; }
}
