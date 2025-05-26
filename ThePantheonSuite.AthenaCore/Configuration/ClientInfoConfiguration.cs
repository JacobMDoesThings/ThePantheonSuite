namespace ThePantheonSuite.AthenaCore.Configuration;

/// <summary>
/// Configuration model defining Azure Active Directory tenant identification information required for authentication.
/// </summary>
public class ClientInfoConfiguration
{
    /// <summary>
    /// Gets the unique identifier representing Azure tenant identity.
    /// </summary>
    public required string TenantId { get; init; }

    /// <summary>
    /// Gets the unique identifier representing Azure client application identity.
    /// </summary>
    public required string ClientId { get; init; }

    /// <summary>
    /// Gets the Azure tenant name used for domain validation purposes.
    /// </summary>
    public required string TenantName { get; init; }
}

/// <summary>
/// Validator implementation ensuring Azure tenant information meets identity format requirements.
/// </summary>
/// <remarks>
/// This validator implements Microsoft.Extensions.Options validation pattern via 
/// <see cref="IValidateOptions{T}"/> interface to ensure Azure tenant identifiers conform 
/// to required format specifications before runtime usage.
/// </remarks>
public partial class ClientInfoConfigurationValidator(ILogger<ClientInfoConfigurationValidator> logger)
    : IValidateOptions<ClientInfoConfiguration>
{
    private readonly ILogger _logger = logger;

    /// <summary>
    /// Validates Azure tenant information meets identity format requirements.
    /// </summary>
    /// <param name="name">Name of the configuration section being validated.</param>
    /// <param name="options"><see cref="ClientInfoConfiguration"/> instance containing Azure tenant identifiers.</param>
    /// <returns><see cref="ValidateOptionsResult"/> indicating validation success or failure reason.</returns>
    public ValidateOptionsResult Validate(string? name, ClientInfoConfiguration options)
    {
        // TenantId validation
        if (!Guid.TryParse(options.TenantId, out _))
        {
            _logger.LogError("Validation failed: Azure TenantId '{TenantId}' must be valid GUID.", options.TenantId);
            return ValidateOptionsResult.Fail("Azure TenantId must be valid GUID.");
        }
        
        // ClientId validation
        if (!Guid.TryParse(options.ClientId, out _))
        {
            _logger.LogError("Validation failed: Azure ClientId '{ClientId}' must be valid GUID.", options.ClientId);
            return ValidateOptionsResult.Fail("Azure ClientId must be valid GUID.");
        }
        
        // TenantName validation
        if (string.IsNullOrEmpty(options.TenantName))
        {
            _logger.LogError("Validation failed: Azure TenantName cannot be empty.");
            return ValidateOptionsResult.Fail("Azure TenantName is required.");
        }

        if (options.TenantName.Length is < 3 or > 64)
        {
            _logger.LogError("Validation failed: Azure TenantName '{TenantName}' must be between 3-64 characters.", options.TenantName);
            return ValidateOptionsResult.Fail("Azure TenantName must be between 3-64 characters.");
        }
        
        if (!TenantNamePatternRegex().IsMatch(options.TenantName))
        {
            _logger.LogError("Validation failed: Azure TenantName '{TenantName}' contains invalid characters.", options.TenantName);
            return ValidateOptionsResult.Fail("Azure TenantName contains invalid characters.");
        }
        
        if (options.TenantName.EndsWith('.'))
        {
            _logger.LogWarning("Azure TenantName '{TenantName}' ends with a dot.", options.TenantName);
        }

        return ValidateOptionsResult.Success;
    }

    /// <summary>
    /// Regex pattern defining valid Azure tenant name characters according to Azure naming conventions.
    /// </summary>
    /// <returns>Precompiled regex pattern matching valid tenant names.</returns>
    [GeneratedRegex("^[a-zA-Z0-9.-]+$")]
    private static partial Regex TenantNamePatternRegex();
}
