using ThePantheonSuite.AthenaCore.Models;
using ThePantheonSuite.GaiaDataStore.Repositories;

namespace ThePantheonSuite.MinervaServices.RepositoryService;

public class CommunityService(IUserCommunityRelationShipRepository userCommunityRepo)
{
    public async Task UpdateUserCommunityRoleAsync(
        string userId,
        string communityId,
        Role newRole)
    {
        await userCommunityRepo.UpdateRoleAsync(userId, communityId, newRole);
    }
    
    public async Task AddUserToCommunityAsync(
        string userId,
        string communityId, Role role)
    {
        await userCommunityRepo.AddUserToCommunityAsync(userId, communityId, role);
    }

}