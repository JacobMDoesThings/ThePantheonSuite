using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.Options;
using ThePantheonSuite.AthenaCore.Models;
using ThePantheonSuite.AthenaCore.Repository;
using ThePantheonSuite.GaiaDataStore.Entities;

namespace ThePantheonSuite.GaiaDataStore.Repositories;

public class UserCommunityRepository(
    CosmosClient cosmosClient,
    CosmosDbSettings settings)
    : CosmosRepository<UserCommunityRelationship>(cosmosClient, settings.DatabaseId,
        settings.UserCommunityContainerName), IUserCommunityRelationShipRepository
{
    private readonly Container _container = cosmosClient.GetContainer(settings.DatabaseId, settings.UserCommunityContainerName);

    public async Task<List<UserCommunityRelationship>> GetByUserIdAsync(string userId)
    {
        var query = _container.GetItemLinqQueryable<UserCommunityRelationship>()
            .Where(c => c.UserId == userId)
            .ToFeedIterator();

        var results = new List<UserCommunityRelationship>();
        
        while (query.HasMoreResults)
        {
            var response = await query.ReadNextAsync();
            results.AddRange(response);
        }

        return results;
    }

    public async Task<List<UserCommunityRelationship>> GetByCommunityIdAsync(string communityId)
    {
        var query = _container.GetItemLinqQueryable<UserCommunityRelationship>()
            .Where(c => c.CommunityId == communityId)
            .ToFeedIterator();

        var results = new List<UserCommunityRelationship>();
        
        while (query.HasMoreResults)
        {
            var response = await query.ReadNextAsync();
            results.AddRange(response);
        }

        return results;
    }

    public async Task AddUserToCommunityAsync(string userId, string communityId)
    {
        var relationship = new UserCommunityRelationship
        {
            Id = $"{userId}_{communityId}",
            UserId = userId,
            CommunityId = communityId,
            Role = Role.Member // Default role
        };

        await CreateAsync(relationship);
    }

    public async Task UpdateRoleAsync(string userId, string communityId, Role newRole)
    {
        var relationship = await GetByUserIdAndCommunityIdAsync(userId, communityId);
        
        if (relationship == null)
            throw new KeyNotFoundException("Relationship not found");

        relationship.Role = newRole;
        await UpdateAsync(relationship);
    }

    public async Task DeleteUserFromCommunityAsync(string userId, string communityId)
    {
        var relationship = await GetByUserIdAndCommunityIdAsync(userId, communityId);
        
        if (relationship == null)
            throw new KeyNotFoundException("Relationship not found");

        await DeleteAsync(relationship.Id, new PartitionKey(userId));
    }

    private async Task<UserCommunityRelationship?> GetByUserIdAndCommunityIdAsync(string userId, string communityId)
    {
        var query = _container.GetItemLinqQueryable<UserCommunityRelationship>()
            .Where(c => c.UserId == userId && c.CommunityId == communityId)
            .ToFeedIterator();

        var results = await query.ReadNextAsync();
        
        return results.FirstOrDefault();
    }

    public async Task DeleteAsync(string id, PartitionKey partitionKey)
    {
        await _container.DeleteItemAsync<UserCommunityRelationship>(id, partitionKey);
    }
}
