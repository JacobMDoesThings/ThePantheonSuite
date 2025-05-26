namespace ThePantheonSuite.AthenaCore.Configuration;

/// <summary>
/// Extension methods for registering Azure service configurations with validation and dependency injection.
/// </summary>
public static class ConfigurationExtensions
{
    /// <summary>
    /// Registers Azure Storage configuration with validation and dependency injection services.
    /// </summary>
    /// <param name="services">Service collection to add configuration services.</param>
    /// <param name="configuration">Application configuration containing Azure Storage settings.</param>
    /// <param name="configSectionName">Configuration section key path containing Azure Storage settings.</param>
    /// <returns>Modified service collection with added configuration services.</returns>
    public static IServiceCollection AddAzureStorageConfiguration(
        this IServiceCollection services, 
        IConfiguration configuration,
        string configSectionName)
    {
        var configSection = configuration.GetRequiredSection(configSectionName);

        services.AddTransient<IValidateOptions<AzureStorageConfiguration>, AzureStorageConfigurationValidator>();

        services.AddOptions<AzureStorageConfiguration>()
            .Bind(configSection)
            .ValidateOnStart();

        services.AddSingleton(provider =>
        {
            try
            {
                var options = provider.GetRequiredService<IOptions<AzureStorageConfiguration>>();
                return options.Value;
            }
            catch (Exception ex)
            {
                var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
                var logger = loggerFactory.CreateLogger<AzureStorageConfigurationValidator>();

                logger.LogError(ex, "Failed to configure Azure Storage configuration.");
                throw;
            }
        });
        
        return services;
    }

    /// <summary>
    /// Registers Cosmos DB configuration with validation and dependency injection services including dynamic indexing policy setup.
    /// </summary>
    /// <param name="services">Service collection to add configuration services.</param>
    /// <param name="configuration">Application configuration containing Cosmos DB settings.</param>
    /// <param name="configSectionName">Configuration section key path containing Cosmos DB settings.</param>
    /// <returns>Modified service collection with added configuration services.</returns>
    public static IServiceCollection AddUserCommunityRelationshipCosmosDbConfiguration(
        this IServiceCollection services,
        IConfiguration configuration, 
        string configSectionName)
    {
        var configSection = configuration.GetRequiredSection(configSectionName);

        services.AddSingleton<IValidateOptions<CosmosDbConfiguration>, CosmosDbConfigurationValidator>();

        services.AddOptions<CosmosDbConfiguration>()
            .Bind(configSection)
            .ValidateOnStart();

        services.AddSingleton(provider =>
        {
            try
            {
                var options = provider.GetRequiredService<IOptions<CosmosDbConfiguration>>();
                var cosmosConfig = options.Value;

                // Build indexing policy from configuration paths
                var indexingPolicy = new IndexingPolicy();
                foreach (var path in cosmosConfig.IncludedPaths)
                {
                    indexingPolicy.IncludedPaths.Add(new IncludedPath { Path = path });
                }

                cosmosConfig.ContainerProperties =
                    new ContainerProperties(cosmosConfig.ContainerName, "userId")
                    {
                        IndexingPolicy = indexingPolicy
                    };

                return cosmosConfig;
            }
            catch (Exception ex)
            {
                var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
                var logger = loggerFactory.CreateLogger<CosmosDbConfigurationValidator>();

                logger.LogError(ex, "Failed to configure CosmosDbContainerProperties.");
                throw;
            }
        });
        
        return services;
    }

    /// <summary>
    /// Registers Azure AD authentication configuration with validation and dependency injection services.
    /// </summary>
    /// <param name="services">Service collection to add configuration services.</param>
    /// <param name="configuration">Application configuration containing Azure AD authentication settings.</param>
    /// <param name="configSectionName">Configuration section key path containing Azure AD authentication settings.</param>
    /// <returns>Modified service collection with added configuration services.</returns>
    public static IServiceCollection AddCustomAzAdAuthenticationConfiguration(
        this IServiceCollection services,
        IConfiguration configuration,
        string configSectionName)
    {
        var configSection = configuration.GetRequiredSection(configSectionName);

        services.AddSingleton<IValidateOptions<ClientInfoConfiguration>, ClientInfoConfigurationValidator>();

        services.AddOptions<ClientInfoConfiguration>()
            .Bind(configSection)
            .ValidateOnStart();

        services.AddSingleton(provider =>
        {
            try
            {
                var options = provider.GetRequiredService<IOptions<ClientInfoConfiguration>>();
                return options.Value;
            }
            catch (Exception ex)
            {
                var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
                var logger = loggerFactory.CreateLogger<ClientInfoConfigurationValidator>();

                logger.LogError(ex, "Failed to configure Azure AD authentication settings.");
                throw;
            }
        });
        
        return services;
    }

    /// <summary>
    /// Registers Blob service configuration with validation and dependency injection services.
    /// </summary>
    /// <param name="services">Service collection to add configuration services.</param>
    /// <param name="configuration">Application configuration containing Blob service settings.</param>
    /// <param name="configSectionName">Configuration section key path containing Blob service settings.</param>
    /// <returns>Modified service collection with added configuration services.</returns>
    public static IServiceCollection AddBlobFileServiceConfiguration(
        this IServiceCollection services, 
        IConfiguration configuration,
        string configSectionName)
    {
        services.AddSingleton<IValidateOptions<BlobServiceConfiguration>, BlobServiceConfigurationValidator>();
        var configSection = configuration.GetRequiredSection(configSectionName);
        services.AddOptions<BlobServiceConfiguration>()
            .Bind(configSection)
            .ValidateOnStart();

        // Register factory method with error handling
        services.AddSingleton(provider =>
        {
            try
            {
                var options = provider.GetRequiredService<IOptions<BlobServiceConfiguration>>();
                return options.Value;
            }
            catch (Exception ex)
            {
                var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
                var logger = loggerFactory.CreateLogger<BlobServiceConfigurationValidator>();

                logger.LogError(ex, "Failed to configure Blob service configuration.");
                throw;
            }
        });
        
        return services;
    }
}