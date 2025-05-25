namespace ThePantheonSuite.AthenaCore.Configuration;

public class AzureStorageConfiguration
{
    
    private readonly string _accountName = string.Empty;
    private readonly string _accountKey = string.Empty;

    public string AccountName 
    { 
        get => _accountName; 
        init 
        {
            if (string.IsNullOrEmpty(value))
                throw new InvalidOperationException("Azure Storage AccountName is required.");
            _accountName = value;
        } 
    }

    public string AccountKey 
    { 
        get => _accountKey; 
        init 
        {
            if (string.IsNullOrEmpty(value))
                throw new InvalidOperationException("Azure Storage AccountKey is required.");
            _accountKey = value;
        } 
    }
}
