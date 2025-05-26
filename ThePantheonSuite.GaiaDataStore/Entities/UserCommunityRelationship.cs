using System.Text.Json.Serialization;
using ThePantheonSuite.AthenaCore.Models;

namespace ThePantheonSuite.GaiaDataStore.Entities;

public class UserCommunityRelationship : BaseEntity
{
    [JsonPropertyName("userId")]
    public required string UserId { get; init; } // Partition key candidate
    
    [JsonPropertyName("communityId")]
    public required string CommunityId { get; init; }
    
    [JsonPropertyName("communityRole")]
    public CommunityRole CommunityRole { get; set; }
    
    [JsonPropertyName("id")]
    public sealed override string Id { get; set; }
    public UserCommunityRelationship()
    {
        Id = $"{UserId}_{CommunityId}";
    }
}
