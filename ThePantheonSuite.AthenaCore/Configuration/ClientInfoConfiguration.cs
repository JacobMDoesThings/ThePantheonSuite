using System.Text.RegularExpressions;

namespace ThePantheonSuite.AthenaCore.Configuration;

public partial class ClientInfoConfiguration
{
    private readonly string _tenantId = string.Empty;
    private readonly string _clientId = string.Empty;
    private readonly string _tenantName = string.Empty;

    public required string TenantId 
    { 
        get => _tenantId; 
        init 
        {
            if (string.IsNullOrEmpty(value))
                throw new InvalidOperationException("Azure TenantId is required.");
            
            // Azure AD tenant IDs are GUIDs
            if (!Guid.TryParse(value, out _))
                throw new InvalidOperationException("Azure TenantId must be a valid GUID.");
                
            _tenantId = value;
        } 
    }

    public required string ClientId 
    { 
        get => _clientId; 
        init 
        {
            if (string.IsNullOrEmpty(value))
                throw new InvalidOperationException("Azure ClientId is required.");
            
            // Azure AD application client IDs are GUIDs
            if (!Guid.TryParse(value, out _))
                throw new InvalidOperationException("Azure ClientId must be a valid GUID.");
                
            _clientId = value;
        } 
    }

    public required string TenantName 
    { 
        get => _tenantName; 
        init 
        {
            if (string.IsNullOrEmpty(value))
                throw new InvalidOperationException("Azure TenantName is required.");
            
            // Azure tenant names typically follow these rules:
            // - Length between 3-64 characters
            // - Alphanumeric with periods and hyphens allowed
            
            if (value.Length is < 3 or > 64)
                throw new InvalidOperationException("Azure TenantName must be between 3-64 characters.");
                
            if (!MyRegex().IsMatch(value))
                throw new InvalidOperationException("Azure TenantName contains invalid characters.");
                
            _tenantName = value;
        } 
    }

    [GeneratedRegex(@"^[a-zA-Z0-9.-]+$")]
    private static partial System.Text.RegularExpressions.Regex MyRegex();
}
