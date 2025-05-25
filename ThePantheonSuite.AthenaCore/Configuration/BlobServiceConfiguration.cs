namespace ThePantheonSuite.AthenaCore.Configuration;

public class BlobServiceConfiguration
{
    public required string? BaseAddress { get; init; }
    public required string? SasWriteEndPoint { get; init; }
    
    public required string? BulkSasWriteEndPoint { get; init; }
    public required string? SasReadEndPoint { get; init; }
}