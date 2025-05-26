namespace ThePantheonSuite.AthenaCore.Configuration;

/// <summary>
/// Configuration model defining Azure Cosmos DB settings required for database operations.
/// </summary>
public class CosmosDbConfiguration
{
    /// <summary>
    /// Gets the throughput capacity measured in Request Units per second (RU/s).
    /// </summary>
    public required int Throughput { get; init; }

    /// <summary>
    /// Gets the name identifier for Azure Cosmos DB database instance.
    /// </summary>
    public required string DatabaseName { get; init; }

    /// <summary>
    /// Gets the name identifier for Azure Cosmos DB container instance.
    /// </summary>
    public required string ContainerName { get; init; }

    /// <summary>
    /// Gets the path identifier specifying partition key structure for data distribution.
    /// </summary>
    public required string PartitionKeyPath { get; init; }

    /// <summary>
    /// Gets optional array specifying paths included in indexing policy overrides.
    /// </summary>
    public required string[] IncludedPaths { get; init; } = [];

    /// <summary>
    /// Gets optional container properties defining advanced configuration options.
    /// </summary>
    public ContainerProperties ContainerProperties { get; set; } = new();
}

/// <summary>
/// Validator implementation ensuring Cosmos DB configuration meets Azure service requirements.
/// </summary>
/// <remarks>
/// This validator implements Microsoft.Extensions.Options validation pattern via 
/// <see cref="IValidateOptions{T}"/> interface to ensure configuration values conform to Azure Cosmos DB 
/// naming conventions and operational constraints before runtime usage.
/// </remarks>
public partial class CosmosDbConfigurationValidator(ILogger<CosmosDbConfigurationValidator> logger)
    : IValidateOptions<CosmosDbConfiguration>
{
    private readonly ILogger _logger = logger;

    /// <summary>
    /// Validates Cosmos DB configuration meets Azure service requirements including throughput limits and naming conventions.
    /// </summary>
    /// <param name="name">Name of the configuration section being validated.</param>
    /// <param name="options"><see cref="CosmosDbConfiguration"/> instance containing database configuration parameters.</param>
    /// <returns><see cref="ValidateOptionsResult"/> indicating validation success or failure reason.</returns>
    public ValidateOptionsResult Validate(string? name, CosmosDbConfiguration options)
    {
        // Throughput validation
        if (options.Throughput is <= 0 or > 10000)
        {
            _logger.LogError("Validation failed: Throughput must be between 1 and 10,000.");
            return ValidateOptionsResult.Fail("Throughput must be between 1 and 10,000 RU/s.");
        }
        
        // Database name validation
        if (string.IsNullOrEmpty(options.DatabaseName))
        {
            _logger.LogError("Validation failed: DatabaseName is required.");
            return ValidateOptionsResult.Fail("DatabaseName is required.");
        }
        
        if (!DatabasePatternRegex().IsMatch(options.DatabaseName))
        {
            _logger.LogError("Validation failed: DatabaseName '{DatabaseName}' contains invalid characters.", options.DatabaseName);
            return ValidateOptionsResult.Fail("DatabaseName must be valid Azure Cosmos DB identifier.");
        }
        
        // Container name validation
        if (string.IsNullOrEmpty(options.ContainerName))
        {
            _logger.LogError("Validation failed: ContainerName is required.");
            return ValidateOptionsResult.Fail("ContainerName is required.");
        }

        if (!DatabasePatternRegex().IsMatch(options.ContainerName))
        {
            _logger.LogError("Validation failed: ContainerName '{ContainerName}' contains invalid characters.", options.ContainerName);
            return ValidateOptionsResult.Fail("ContainerName must be valid Azure Cosmos DB identifier.");
        }
        
        // Partition key path validation
        if (string.IsNullOrEmpty(options.PartitionKeyPath))
        {
            _logger.LogError("Validation failed: PartitionKeyPath is required.");
            return ValidateOptionsResult.Fail("PartitionKeyPath is required.");
        }
        
        if (!PartitionKeyRegex().IsMatch(options.PartitionKeyPath))
        {
            _logger.LogError("Validation failed: PartitionKeyPath '{PartitionKeyPath}' is invalid.", options.PartitionKeyPath);
            return ValidateOptionsResult.Fail("PartitionKeyPath must start with '/' followed by valid identifiers.");
        }
        
        // Partition key path formatting check
        if (options.PartitionKeyPath.Length > 1 && options.PartitionKeyPath[^1] == '/')
        {
            _logger.LogWarning("PartitionKeyPath '{PartitionKeyPath}' ends with a slash.", options.PartitionKeyPath);
        }

        return ValidateOptionsResult.Success;
    }

    /// <summary>
    /// Regex pattern validating Azure Cosmos DB partition key path format requirements.
    /// </summary>
    /// <returns>Precompiled regex pattern matching valid partition key paths.</returns>
    [GeneratedRegex(@"^\/([a-zA-Z0-9_\-\.]+)(\/[a-zA-Z0-9_\-\.]+)*$")]
    private static partial Regex PartitionKeyRegex();
    
    /// <summary>
    /// Regex pattern validating Azure Cosmos DB identifier naming conventions.
    /// </summary>
    /// <returns>Precompiled regex pattern matching valid database/container names.</returns>
    [GeneratedRegex("""^[a-zA-Z0-9_\-\.~!@#$%^&*()+=\{\}$$|:;'""<>,.?/ ]{1,255}$""")]
    private static partial Regex DatabasePatternRegex();
}
