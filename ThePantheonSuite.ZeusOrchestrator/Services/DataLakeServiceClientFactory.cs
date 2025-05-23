using Azure.Storage.Files.DataLake;
using ThePantheonSuite.ZeusOrchestrator.Interfaces;

namespace ThePantheonSuite.ZeusOrchestrator.Services;

public class DataLakeServiceClientFactory : IDataLakeServiceClientFactory
{
    public DataLakeServiceClient Create(string connectionString)
    {
        return new DataLakeServiceClient(connectionString);
    }
}
