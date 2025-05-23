namespace ThePantheonSuite.AthenaCore.AuthzAuthn;

public class ClientInfoConfiguration
{
    public required string TenantId { get; init; }
    public required string ClientId { get; init; }
    public required string TenantName { get; init; }
}