using Net.Shared.Persistence.Models.Settings.Connections.Base;

namespace Net.Shared.Persistence.Models.Settings.Connections;

public sealed record MongoConnectionSettings : NetSharedPersistenceConnectionSettings
{
    public override string ConnectionString => $"mongodb://{User}:{Password}@{Host}:{Port}/?authMechanism=SCRAM-SHA-256";
}