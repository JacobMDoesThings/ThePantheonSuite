namespace ThePantheonSuite.AthenaCore.Configuration;

public class AzureAdConfiguration
{
    public required string ClientId { get; init; }
    public required string ClientSecret { get; init; }
    public required string TenantId { get; init; }
}