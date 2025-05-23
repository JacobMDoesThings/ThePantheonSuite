using Azure.Storage.Files.DataLake;

namespace ThePantheonSuite.ZeusOrchestrator.Interfaces;

public interface IDataLakeServiceClientFactory
{
    DataLakeServiceClient Create(string connectionString);
}