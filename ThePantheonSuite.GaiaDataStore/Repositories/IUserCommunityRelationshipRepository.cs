using ThePantheonSuite.AthenaCore.Models;
using ThePantheonSuite.GaiaDataStore.Entities;

namespace ThePantheonSuite.GaiaDataStore.Repositories;

public interface IUserCommunityRelationShipRepository : IRepository<UserCommunityRelationship>
{
    Task<List<UserCommunityRelationship>> GetByUserIdAsync(string userId);
    
    Task<List<UserCommunityRelationship>> GetByCommunityIdAsync(string communityId);
    
    Task AddUserToCommunityAsync(string userId, string communityId, CommunityRole communityRole = CommunityRole.Member);
    
    Task UpdateRoleAsync(string userId, string communityId, CommunityRole newCommunityRole);
    
    Task DeleteUserFromCommunityAsync(string userId, string communityId);
    
}
