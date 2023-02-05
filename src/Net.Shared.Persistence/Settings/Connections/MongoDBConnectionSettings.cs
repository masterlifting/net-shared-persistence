namespace Shared.Persistence.Settings.Connections;

public sealed record MongoDBConnectionSettings
{
    public string Host { get; set; } = null!;
    public int Port { get; set; }
    public string Database { get; set; } = null!;
    public string User { get; set; } = null!;
    public string Password { get; set; } = null!;

    public string GetConnectionString() => $"mongodb://{User}:{Password}@{Host}:{Port}/?authMechanism=SCRAM-SHA-256";
}