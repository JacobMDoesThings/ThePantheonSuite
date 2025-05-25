using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ThePantheonSuite.AthenaCore.Configuration;

public static class ConfigurationExtensions
{
    public static void AddAzureStorageConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        var configSection = configuration.GetRequiredSection(nameof(AzureStorageConfiguration));
        services.Configure<AzureStorageConfiguration>(configSection);
        var azureConfig = configSection.Get<AzureStorageConfiguration>();

        if (azureConfig is null)
            throw new InvalidOperationException("Azure storage configuration must not be null.");
        
        services.AddSingleton(azureConfig);
    }

    public static void AddUserCommunityRelationshipCosmosDbConfiguration(this IServiceCollection services,
        IConfiguration configuration)
    {
        var configSection = configuration.GetRequiredSection($"CommunityRelationship{nameof(CosmosDbConfiguration)}");
        services.Configure<CosmosDbConfiguration>(configSection);
        var cosmosDbConfiguration = configSection.Get<CosmosDbConfiguration>();

        if (cosmosDbConfiguration is null)
            throw new InvalidOperationException("Cosmos Db configuration must not be null.");
        
        var indexingPolicy = new IndexingPolicy();
        foreach (var path in cosmosDbConfiguration.IncludedPaths)
        {
            indexingPolicy.IncludedPaths.Add(new IncludedPath { Path = path });
        }

        cosmosDbConfiguration!.ContainerProperties =
            new ContainerProperties(cosmosDbConfiguration.ContainerName, "userId");
        cosmosDbConfiguration.ContainerProperties.IndexingPolicy = indexingPolicy;

        services.AddSingleton(cosmosDbConfiguration);
    }
    public static void AddCustomAzAdAuthentication(this IServiceCollection services, 
        IConfiguration configuration,
        string clientSectionName)
    {
        var configSection = configuration.GetRequiredSection(clientSectionName);
        services.Configure<ClientInfoConfiguration>(configSection);
        var clientInfoConfig = configSection.Get<ClientInfoConfiguration>();
        if(clientInfoConfig is null)
            throw new InvalidOperationException("ClientInfoConfiguration is missing");
        
        services.AddSingleton(clientInfoConfig);
    }
}
