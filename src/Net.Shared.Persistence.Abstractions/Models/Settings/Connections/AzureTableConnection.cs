using Net.Shared.Persistence.Abstractions.Models.Settings.Connections.Base;

namespace Net.Shared.Persistence.Abstractions.Models.Settings.Connections;

public sealed record AzureTableConnection : PersistenceConnection
{
    public const string SectionName = "AzureTableConnection";
    public override string ConnectionString => $"DefaultEndpointsProtocol=https;AccountName={User};AccountKey={Password};EndpointSuffix=core.windows.net";
}
