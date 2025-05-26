using ThePantheonSuite.AthenaCore.Models;
using ThePantheonSuite.GaiaDataStore.Repositories;

namespace ThePantheonSuite.MinervaServices.RepositoryService;

public class CommunityService(IUserCommunityRelationShipRepository userCommunityRepo)
{
    public async Task UpdateUserCommunityRoleAsync(
        string userId,
        string communityId,
        CommunityRole newCommunityRole)
    {
        await userCommunityRepo.UpdateRoleAsync(userId, communityId, newCommunityRole);
    }
    
    public async Task AddUserToCommunityAsync(
        string userId,
        string communityId, CommunityRole communityRole)
    {
        await userCommunityRepo.AddUserToCommunityAsync(userId, communityId, communityRole);
    }

}