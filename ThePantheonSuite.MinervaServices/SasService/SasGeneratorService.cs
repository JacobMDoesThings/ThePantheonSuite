using System.Security.Claims;
using Azure;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using ThePantheonSuite.AthenaCore.Interfaces;
using ThePantheonSuite.AthenaCore.SasService;

namespace ThePantheonSuite.MinervaServices.SasService;

public class SasGeneratorService(AzureStorageConfiguration azStConfiguration) : ISasGeneratorService

{
    public async Task<SasUrlResponse> GenerateWriteSasTokenAsync(
        ClaimsPrincipal principal, SasGenerationRequestModel sasGenerationRequest)
    {
        try
        {
            sasGenerationRequest.SetPrincipal(principal);
            var containerClient = await GetBlobContainerClientAsync(GetBlobServiceClient(), sasGenerationRequest);
            return GenerateSasUrl(containerClient, sasGenerationRequest, BlobSasPermissions.Write);
        }
       
        catch (Exception ex)
        {
            throw new SasGeneratorException("Failed to generate write sas url", ex);
        }
    }

    public async Task<IEnumerable<SasUrlResponse>> BulkGenerateWriteSasTokenAsync(
        SasGenerationRequestModel sasGenerationRequestModel, int count)
    {
        List<SasUrlResponse> sasUrls = [];
        try
        {
            var blobServiceClient = await GetBlobContainerClientAsync(GetBlobServiceClient(), sasGenerationRequestModel);
            for (var i = 0; i < count; i++)
            {
                sasUrls.Add(GenerateSasUrl(blobServiceClient, sasGenerationRequestModel, BlobSasPermissions.Write));
            }
            return sasUrls;
        }
        catch (Exception ex)
        {
            throw new SasGeneratorException("Failed to generate write sas url during bulk transaction", ex);
        }
    }
    

    public async Task<SasUrlResponse> GenerateReadSasTokenAsync(SasGenerationRequestModel sasGenerationRequestModel, string imageName)
    {
        try
        {
            var containerClient = await GetBlobContainerClientAsync(GetBlobServiceClient(), sasGenerationRequestModel);
            return GenerateSasUrl(containerClient, sasGenerationRequestModel, BlobSasPermissions.Read, imageName);
        }
        catch (Exception ex)
        {
            throw new SasGeneratorException("Failed to read blob", ex);
        }
    }

    private SasUrlResponse GenerateSasUrl(BlobContainerClient blobContainerClient, 
        SasGenerationRequestModel sasGenerationRequestModel, BlobSasPermissions permission, string blobName = "")
    {
        blobName = permission switch
        {
            BlobSasPermissions.Write => GenerateBlobName(sasGenerationRequestModel),
            _ => blobName
        };

        //var blobName = GenerateBlobName(sasGenerationRequestModel);
        var blobClient = blobContainerClient.GetBlobClient(blobName);
        var sasBuilder = new BlobSasBuilder()
        {
            BlobContainerName = sasGenerationRequestModel.SelectedGroupId,
            BlobName = blobName,
            Resource = "b",
            ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(10),
        };

        sasBuilder.SetPermissions(permission);

        var sasToken = sasBuilder.ToSasQueryParameters(
            new StorageSharedKeyCredential(azStConfiguration.AccountName, azStConfiguration.AccountKey)).ToString();
        
        return new SasUrlResponse
        { 
            SasToken = $"{blobClient.Uri}?{sasToken}",
            BlobName = blobName
            
        };
    }

    private BlobServiceClient GetBlobServiceClient()
    {
        var blobServiceClient = new BlobServiceClient(
            new Uri($"https://{azStConfiguration.AccountName}.blob.core.windows.net"),
            new StorageSharedKeyCredential(azStConfiguration.AccountName, azStConfiguration.AccountKey)
        );
        
        return blobServiceClient;
    }

    private static async Task<BlobContainerClient> GetBlobContainerClientAsync(BlobServiceClient blobServiceClient,
        SasGenerationRequestModel sasGenerationRequestModel)
    {
        var containerClient = blobServiceClient.GetBlobContainerClient(sasGenerationRequestModel.SelectedGroupId);

        var createResponse = await containerClient.CreateIfNotExistsAsync();

        if (createResponse?.GetRawResponse().Status != 201 && createResponse is not null)
        {
            throw new SasGeneratorException($"Exception while creating blob container: " +
                                            $"{createResponse.GetRawResponse().Content}");
        }
        if (createResponse is not null)
        {
            await CheckContainerAvailabilityAsync(containerClient);
        }
        
        return containerClient;
    }

    private static async Task CheckContainerAvailabilityAsync(BlobContainerClient blobContainerClient)
    {
        var containerAvailable = false;
        var retryCount = 0;
        var delay = TimeSpan.FromSeconds(1);

        while (!containerAvailable && retryCount < 5)
        {
            try
            {
                await blobContainerClient.GetPropertiesAsync();
                containerAvailable = true;
            }
            catch (RequestFailedException ex) when (
                ex.Status is 404)
            {
                retryCount++;
                await Task.Delay(delay);
                delay = TimeSpan.FromMilliseconds(delay.TotalMilliseconds);
            }
        }

        if (!containerAvailable)
        {
            throw new SasGeneratorException("Container not available after retries.");
        }
    }

    private static string GenerateBlobName(SasGenerationRequestModel sasGenerationRequestModel)
    {
        return  sasGenerationRequestModel.IsPublic
            ? $"public/images/{sasGenerationRequestModel.UserId}/{Guid.NewGuid()}-{DateTime.Now.Ticks}"
            : $"{sasGenerationRequestModel.UserId}/images/{Guid.NewGuid()}-{DateTime.Now.Ticks}";
    }

    private class SasGeneratorException : Exception
    {
        public SasGeneratorException(string message):base(message){}
        public SasGeneratorException(string message, Exception innerException):base(message, innerException){}
    }
}