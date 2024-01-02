using Net.Shared.Persistence.Abstractions.Models.Settings.Connections.Base;

namespace Net.Shared.Persistence.Abstractions.Models.Settings.Connections;

public sealed record MongoDbConnectionSettings : PersistenceConnectionSettings
{
    public const string SectionName = "MongoDbConnection";
    public override string ConnectionString => $"mongodb://{User}:{Password}@{Host}:{Port}/?directConnection=true&authSource=admin";
}
