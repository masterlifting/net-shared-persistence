using Net.Shared.Persistence.Abstractions.Models.Settings.Connections.Base;

namespace Net.Shared.Persistence.Abstractions.Models.Settings.Connections;

public sealed record PostgreSqlConnection : PersistenceConnection
{
    public override string ConnectionString => $"Server={Host};Port={Port};Database={Database};UserId={User};Password={Password}";
}