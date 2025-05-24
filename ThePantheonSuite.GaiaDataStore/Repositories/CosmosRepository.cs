using Microsoft.Azure.Cosmos;
using ThePantheonSuite.GaiaDataStore.Entities;

namespace ThePantheonSuite.GaiaDataStore.Repositories;

public class CosmosRepository<T> : IRepository<T> where T : BaseEntity
{
    private readonly Container _container;

    public CosmosRepository(
        CosmosClient cosmosClient,
        string databaseId,
        string containerName)
    {
        var database = cosmosClient.GetDatabase(databaseId);
        _container = database.GetContainer(containerName);
    }

    public async Task<T?> GetAsync(string id)
    {
        try
        {
            var response = await _container.ReadItemAsync<T>(id, new PartitionKey(id));
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task CreateAsync(T entity)
    {
        var response = await _container.CreateItemAsync(
            entity, 
            new PartitionKey(entity.Id),
            new ItemRequestOptions { EnableContentResponseOnWrite = true }
        );
    }

    public async Task UpdateAsync(T entity)
    {
        var response = await _container.ReplaceItemAsync(
            entity, 
            entity.Id,
            new PartitionKey(entity.Id),
            new ItemRequestOptions { IfMatchEtag = entity.ETag }
        );
    }
}



