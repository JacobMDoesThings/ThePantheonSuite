using System.Text.Json.Serialization;

namespace ThePantheonSuite.AthenaCore.SasService;

public class SasUrlResponse
{
    [JsonPropertyName("sasToken")]
    public required string SasToken { get; init; }
    [JsonPropertyName("blobName")]
    public required string BlobName { get; init; }
}