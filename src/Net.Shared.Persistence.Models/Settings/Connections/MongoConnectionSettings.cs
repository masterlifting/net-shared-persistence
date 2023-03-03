namespace Net.Shared.Persistence.Models.Settings.Connections;

public sealed record MongoConnectionSettings : ConnectionSettings
{
    public override string GetConnectionString() => $"mongodb://{User}:{Password}@{Host}:{Port}/?authMechanism=SCRAM-SHA-256";
}