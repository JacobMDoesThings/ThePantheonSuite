using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ThePantheonSuite.AthenaCore.SasService;

public static class ConfigurationExtensions
{
    public static void AddAzureStorageConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        var configSection = configuration.GetRequiredSection("AzureStorage");
        services.Configure<AzureStorageConfiguration>(configSection);
        var azureConfig = configSection.Get<AzureStorageConfiguration>();

        if (string.IsNullOrEmpty(azureConfig?.AccountName))
            throw new InvalidOperationException("Azure Storage AccountName is required.");

        if (string.IsNullOrEmpty(azureConfig?.AccountKey))
            throw new InvalidOperationException("Azure Storage AccountKey is required.");
        
        services.AddSingleton(azureConfig);
    }
}
