using System.Text.Json.Serialization;
using ThePantheonSuite.AthenaCore.Models;

namespace ThePantheonSuite.GaiaDataStore.Entities;

public class UserCommunityRelationship : BaseEntity
{
    [JsonPropertyName("userId")]
    public required string UserId { get; init; } // Partition key candidate
    
    [JsonPropertyName("communityId")]
    public required string CommunityId { get; init; }
    
    [JsonPropertyName("role")]
    public Role Role { get; set; }
}
