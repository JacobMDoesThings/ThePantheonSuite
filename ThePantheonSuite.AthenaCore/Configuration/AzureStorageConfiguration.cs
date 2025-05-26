namespace ThePantheonSuite.AthenaCore.Configuration;

/// <summary>
/// Configuration model representing Azure Storage account credentials required for service integration.
/// </summary>
public class AzureStorageConfiguration
{
    /// <summary>
    /// Gets the name of the Azure Storage account required for service connection.
    /// </summary>
    public required string AccountName { get; init; }

    /// <summary>
    /// Gets the access key for authenticating requests to Azure Storage services.
    /// </summary>
    public required string AccountKey { get; init; }
}

/// <summary>
/// Validator implementation ensuring Azure Storage configuration contains required credentials.
/// </summary>
/// <remarks>
/// This validator implements the Microsoft.Extensions.Options validation pattern using 
/// <see cref="IValidateOptions{T}"/> interface to ensure configuration values meet service requirements 
/// before runtime usage.
/// </remarks>
public class AzureStorageConfigurationValidator(ILogger<AzureStorageConfigurationValidator> logger)
    : IValidateOptions<AzureStorageConfiguration>
{
    private readonly ILogger _logger = logger;

    /// <summary>
    /// Validates Azure Storage configuration contains required account credentials.
    /// </summary>
    /// <param name="name">Name of the configuration section being validated.</param>
    /// <param name="options"><see cref="AzureStorageConfiguration"/> instance containing Azure Storage credentials.</param>
    /// <returns><see cref="ValidateOptionsResult"/> indicating validation success or failure reason.</returns>
    public ValidateOptionsResult Validate(string? name, AzureStorageConfiguration options)
    {
        // Check required account identifier
        if (string.IsNullOrEmpty(options.AccountName))
        {
            _logger.LogError("Validation failed: Azure Storage AccountName is required.");
            return ValidateOptionsResult.Fail("Azure Storage AccountName is required.");
        }

        // Check required authentication credential
        if (!string.IsNullOrEmpty(options.AccountKey)) 
            return ValidateOptionsResult.Success;
        
        _logger.LogError("Validation failed: Azure Storage AccountKey is required.");
        return ValidateOptionsResult.Fail("Azure Storage AccountKey is required.");
    }
}
