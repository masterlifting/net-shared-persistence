using Net.Shared.Persistence.Models.Settings.Connections.Base;

namespace Net.Shared.Persistence.Models.Settings.Connections;

public sealed record PostgreConnectionSettings : NetSharedPersistenceConnectionSettings
{
    public override string ConnectionString => $"Server={Host};Port={Port};Database={Database};UserId={User};Password={Password}";
}