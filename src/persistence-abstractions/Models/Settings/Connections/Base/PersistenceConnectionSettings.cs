using System.ComponentModel.DataAnnotations;
using Net.Shared.Abstractions.Models.Settings.Connection;

namespace Net.Shared.Persistence.Abstractions.Models.Settings.Connections.Base;

public abstract record PersistenceConnectionSettings : ServerConnectionSettings
{
    [Required]
    public string Database { get; set; } = null!;
}
