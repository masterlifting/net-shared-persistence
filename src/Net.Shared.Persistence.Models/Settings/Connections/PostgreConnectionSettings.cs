namespace Net.Shared.Persistence.Models.Settings.Connections;

public sealed record PostgreConnectionSettings : ConnectionSettings
{
    public override string GetConnectionString() => $"Server={Host};Port={Port};Database={Database};UserId={User};Password={Password}";
}