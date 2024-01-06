using System.ComponentModel.DataAnnotations;
using Net.Shared.Abstractions.Models.Settings.Connection;

namespace Net.Shared.Persistence.Abstractions.Models.Settings.Connections;

public sealed record AzureTableConnectionSettings : SecretConnectionSettings
{
    public const string SectionName = "AzureTableConnection";
    [Required] public string AccountName { get; init; } = null!;
    public override string ConnectionString => $"DefaultEndpointsProtocol=https;AccountName={AccountName};AccountKey={SecretKey};EndpointSuffix=core.windows.net";
}
