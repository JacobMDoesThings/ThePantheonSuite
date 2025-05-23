namespace ThePantheonSuite.AthenaCore.SasService;

public class AzureStorageConfiguration
{
    public required string AccountName { get; init; }
    public required string AccountKey { get; init; }

    internal bool IsValid()
        => !string.IsNullOrEmpty(AccountName) &&
           !string.IsNullOrEmpty(AccountKey);
}