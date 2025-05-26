using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.Options;
using ThePantheonSuite.AthenaCore.Configuration;
using ThePantheonSuite.AthenaCore.Models;
using ThePantheonSuite.GaiaDataStore.Entities;

namespace ThePantheonSuite.GaiaDataStore.Repositories;

public class UserCommunityRelationshipRepository
    : CosmosRepository<UserCommunityRelationship>, IUserCommunityRelationShipRepository
{
    private readonly Container? _container;

    public UserCommunityRelationshipRepository(CosmosClient cosmosClient, 
        CosmosDbConfiguration configuration) : base(cosmosClient, configuration.DatabaseName,
        configuration.ContainerName)
    {
        try
        {
            var database = cosmosClient.CreateDatabaseIfNotExistsAsync(configuration.DatabaseName).Result.Database;
            // Use built-in method instead of manual existence check
            database.CreateContainerIfNotExistsAsync(
                configuration.ContainerProperties,
                throughput: configuration.Throughput);

            _container = cosmosClient.GetContainer(configuration.DatabaseName, configuration.ContainerName);
        }
        catch (CosmosException ex) when (
            ex.StatusCode == System.Net.HttpStatusCode.Conflict ||
            ex.StatusCode == System.Net.HttpStatusCode.BadRequest)
        {
            // Handle cases where container already exists or invalid configuration
            Console.WriteLine($"Container creation failed: {ex.Message}");
        }
    }
    
    public async Task<List<UserCommunityRelationship>> GetByUserIdAsync(string userId)
    {
        var query = _container?.GetItemLinqQueryable<UserCommunityRelationship>()
            .Where(c => c.UserId == userId)
            .ToFeedIterator();

        var results = new List<UserCommunityRelationship>();

        while (query!.HasMoreResults)
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

    public async Task AddUserToCommunityAsync(string userId, string communityId, CommunityRole communityRole = CommunityRole.Member)
    {
        var relationship = new UserCommunityRelationship
        {
            Id = $"{userId}_{communityId}",
            UserId = userId,
            CommunityId = communityId,
            CommunityRole = communityRole
        };

        await CreateAsync(relationship);
    }

    public async Task UpdateRoleAsync(string userId, string communityId, CommunityRole newCommunityRole)
    {
        var relationship = await GetByUserIdAndCommunityIdAsync(userId, communityId);

        if (relationship == null)
            throw new KeyNotFoundException("Relationship not found");

        relationship.CommunityRole = newCommunityRole;
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
        var query = _container?.GetItemLinqQueryable<UserCommunityRelationship>()
            .Where(c => c.UserId == userId && c.CommunityId == communityId)
            .ToFeedIterator();
        
        var results = await query?.ReadNextAsync();

        return results.FirstOrDefault();
    }

    public async Task DeleteAsync(string id, PartitionKey partitionKey)
    {
        if (_container is not null)
            await (_container.DeleteItemAsync<UserCommunityRelationship>(id, partitionKey));
    }
}