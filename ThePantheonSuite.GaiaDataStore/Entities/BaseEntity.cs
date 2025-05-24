namespace ThePantheonSuite.GaiaDataStore.Entities;

using System.Text.Json.Serialization;

public abstract class BaseEntity
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonPropertyName("_etag")]
    public string ETag { get; set; } = null!;
}
