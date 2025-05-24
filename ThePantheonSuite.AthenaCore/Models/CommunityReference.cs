namespace ThePantheonSuite.AthenaCore.Models;

public class CommunityReference
{
    public required string CommunityId { get; init; }
    
    public Role Role { get; set; }
}