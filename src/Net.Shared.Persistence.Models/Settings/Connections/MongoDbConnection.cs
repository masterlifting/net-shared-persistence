﻿using Net.Shared.Persistence.Models.Settings.Connections.Base;

namespace Net.Shared.Persistence.Models.Settings.Connections;

public sealed record MongoDbConnection : PersistenceConnection
{
    public override string ConnectionString => $"mongodb://{User}:{Password}@{Host}:{Port}/?directConnection=true&authSource=admin";
}