namespace ThePantheonSuite.AthenaCore.Configuration;

/// <summary>
/// Configuration model defining Azure Blob Service endpoints required for storage operations.
/// </summary>
public class BlobServiceConfiguration
{
    /// <summary>
    /// Gets the base URL segment serving as root endpoint for blob operations.
    /// </summary>
    public required string BaseAddress { get; init; }

    /// <summary>
    /// Gets the Shared Access Signature endpoint for single-blob write operations.
    /// </summary>
    public required string SasWriteEndPoint { get; init; }

    /// <summary>
    /// Gets the Shared Access Signature endpoint optimized for bulk write operations.
    /// </summary>
    public required string BulkSasWriteEndPoint { get; init; }

    /// <summary>
    /// Gets the Shared Access Signature endpoint for read operations.
    /// </summary>
    public required string SasReadEndPoint { get; init; }
}

/// <summary>
/// Validator implementation ensuring Blob Service configuration endpoints meet URL format requirements.
/// </summary>
/// <remarks>
/// This validator implements the Microsoft.Extensions.Options validation pattern via 
/// <see cref="IValidateOptions{T}"/> interface to ensure endpoint URLs conform to valid URL segment patterns 
/// before runtime usage.
/// </remarks>
public partial class BlobServiceConfigurationValidator(ILogger<BlobServiceConfigurationValidator> logger)
    : IValidateOptions<BlobServiceConfiguration>
{
    private readonly ILogger _logger = logger;

    /// <summary>
    /// Validates Blob Service configuration endpoints meet URL format requirements.
    /// </summary>
    /// <param name="name">Name of the configuration section being validated.</param>
    /// <param name="options"><see cref="BlobServiceConfiguration"/> instance containing endpoint URLs.</param>
    /// <returns><see cref="ValidateOptionsResult"/> indicating validation success or failure reason.</returns>
    public ValidateOptionsResult Validate(string? name, BlobServiceConfiguration options)
    {
        // BaseAddress validation
        if (string.IsNullOrEmpty(options.BaseAddress))
        {
            _logger.LogError("Validation failed: BaseAddress cannot be empty.");
            return ValidateOptionsResult.Fail("BaseAddress is required.");
        }

        if (!ValidateUrlSegment(options.BaseAddress))
        {
            _logger.LogError("Validation failed: BaseAddress '{BaseAddress}' contains invalid characters.", options.BaseAddress);
            return ValidateOptionsResult.Fail("BaseAddress must be valid URL segment.");
        }
        
        // SasWriteEndPoint validation
        if (string.IsNullOrEmpty(options.SasWriteEndPoint))
        {
            _logger.LogError("Validation failed: SasWriteEndPoint cannot be empty.");
            return ValidateOptionsResult.Fail("SasWriteEndPoint is required.");
        }

        if (!ValidateUrlSegment(options.SasWriteEndPoint))
        {
            _logger.LogError("Validation failed: SasWriteEndPoint '{SasWriteEndPoint}' contains invalid characters.", options.SasWriteEndPoint);
            return ValidateOptionsResult.Fail("SasWriteEndPoint must be valid URL segment.");
        }
        
        // BulkSasWriteEndPoint validation
        if (string.IsNullOrEmpty(options.BulkSasWriteEndPoint))
        {
            _logger.LogError("Validation failed: BulkSasWriteEndPoint cannot be empty.");
            return ValidateOptionsResult.Fail("BulkSasWriteEndPoint is required.");
        }

        if (!ValidateUrlSegment(options.BulkSasWriteEndPoint))
        {
            _logger.LogError("Validation failed: BulkSasWriteEndPoint '{BulkSasWriteEndPoint}' contains invalid characters.", options.BulkSasWriteEndPoint);
            return ValidateOptionsResult.Fail("BulkSasWriteEndPoint must be valid URL segment.");
        }
        
        // SasReadEndPoint validation
        if (string.IsNullOrEmpty(options.SasReadEndPoint))
        {
            _logger.LogError("Validation failed: SasReadEndPoint cannot be empty.");
            return ValidateOptionsResult.Fail("SasReadEndPoint is required.");
        }

        if (!ValidateUrlSegment(options.SasReadEndPoint))
        {
            _logger.LogError("Validation failed: SasReadEndPoint '{SasReadEndPoint}' contains invalid characters.", options.SasReadEndPoint);
            return ValidateOptionsResult.Fail("SasReadEndPoint must be valid URL segment.");
        }
        
        // Optional endpoint pattern validation
        foreach (var endpoint in new[] { options.SasWriteEndPoint, options.BulkSasWriteEndPoint })
        {
            if (!endpoint.EndsWith('/') && !endpoint.Contains('?'))
            {
                _logger.LogWarning("Endpoint '{Endpoint}' does not end with slash or query parameters.", endpoint);
            }
        }

        return ValidateOptionsResult.Success;
    }

    /// <summary>
    /// Validates URL segments conform to valid URI character patterns.
    /// </summary>
    /// <param name="segment">URL segment string to validate.</param>
    /// <returns>Boolean indicating valid URL pattern compliance.</returns>
    private static bool ValidateUrlSegment(string segment)
    {
        const int maxLength = 2048;
        
        return segment.Length <= maxLength && PatternRegex().IsMatch(segment);
    }

    /// <summary>
    /// Regex pattern defining valid URL segment characters according to RFC 3986 specification.
    /// </summary>
    /// <returns>Precompiled regex pattern matching valid URL segments.</returns>
    [GeneratedRegex(@"^[a-zA-Z0-9\-._~:\/?#$$$$@!$&'()*+,;=]+$")]
    private static partial Regex PatternRegex();
}
