using Microsoft.Azure.Cosmos;

namespace ThePantheonSuite.AthenaCore.Configuration;

public class CosmosDbConfiguration
{
    private readonly int _throughput;
    private readonly string _databaseName = string.Empty;
    private readonly string _containerName = string.Empty;

    public required int Throughput 
    { 
        get => _throughput; 
        init 
        {
            if (value is <= 0 or > 10000)
                throw new InvalidOperationException("Throughput must be between 1 and 10000 (inclusive).");
            _throughput = value;
        } 
    }

    public required string DatabaseName 
    { 
        get => _databaseName; 
        init 
        {
            if (string.IsNullOrEmpty(value))
                throw new InvalidOperationException("DatabaseName is required.");
            _databaseName = value;
        } 
    }

    public required string ContainerName 
    { 
        get => _containerName; 
        init 
        {
            if (string.IsNullOrEmpty(value))
                throw new InvalidOperationException("ContainerName is required.");
            _containerName = value;
        } 
    }

    public required string[] IncludedPaths { get; init; } = [];
    
    public ContainerProperties ContainerProperties { get; set; } = new ContainerProperties();
}

