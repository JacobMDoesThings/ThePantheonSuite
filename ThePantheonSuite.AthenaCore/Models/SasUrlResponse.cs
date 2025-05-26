namespace ThePantheonSuite.AthenaCore.Models;

/// <summary>
/// Response model containing SAS token information for blob access.
/// </summary>
/// <remarks>
/// This class represents the data returned from SAS generation services containing both 
/// the security token and associated blob identifier.
/// </remarks>
public class SasUrlResponse
{
    /// <summary>
    /// Gets the SAS token granting access permissions to Azure Blob Storage.
    /// </summary>
    [JsonPropertyName("sasToken")]
    public required string SasToken { get; init; }

    /// <summary>
    /// Gets the name of the blob file associated with this SAS token.
    /// </summary>
    [JsonPropertyName("blobName")]
    public required string BlobName { get; init; }
}