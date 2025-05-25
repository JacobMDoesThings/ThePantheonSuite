using System.Text.Json.Serialization;

namespace ThePantheonSuite.AthenaCore.Models;

public class SasUrlResponse
{
    [JsonPropertyName("sasToken")]
    public required string SasToken { get; init; }
    [JsonPropertyName("blobName")]
    public required string BlobName { get; init; }
}