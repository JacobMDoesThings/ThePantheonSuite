namespace ThePantheonSuite.AthenaCore.Repository;

public class CosmosDbSettings
{
    public required string ConnectionString { get; set; }
    
    public required string DatabaseId { get; set; }
    
    public required string UserCommunityContainerName { get; set; }
}
