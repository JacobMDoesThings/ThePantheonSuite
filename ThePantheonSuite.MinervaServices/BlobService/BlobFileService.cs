using System.Net.Http.Json;
using System.Text.Json;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using ThePantheonSuite.AthenaCore.Configuration.BlobService;
using ThePantheonSuite.AthenaCore.Interfaces;
using ThePantheonSuite.AthenaCore.SasService;


namespace ThePantheonSuite.MinervaServices.BlobService;

public class BlobFileService : IFileUploadService
{
    private readonly HttpClient _sasServiceClient;
    private readonly BlobServiceConfiguration _blobServiceConfiguration;

    public BlobFileService(BlobServiceConfiguration configuration, IHttpClientFactory httpClientFactory)
    {
        _sasServiceClient = httpClientFactory.CreateClient("SasService");
        if (configuration.BaseAddress is not null) _sasServiceClient.BaseAddress = new Uri(configuration.BaseAddress);
        _blobServiceConfiguration = configuration;
    }

    public async Task<SasUrlResponse?> UploadImageAsync(Stream fileStream, bool isPublic = false)
    {
        // var groupId = "groupid1";
        // var requestContent = new StringContent(
        //     JsonConvert.SerializeObject(new { selectedGroupId = groupId, isPublic }),
        //     Encoding.UTF8,
        //     "application/json"
        // );
        //
        // var response = await _sasServiceClient.PostAsync(_blobServiceConfiguration.SasWriteEndPoint, requestContent);

        var groupId = "groupid1";
        var urlWithParams = $"{_blobServiceConfiguration.SasWriteEndPoint}?selectedGroupId={groupId}&isPublic={isPublic}";
        var response = await _sasServiceClient.GetAsync(urlWithParams);
        if (!response.IsSuccessStatusCode) return null;
        var result = await response.Content.ReadFromJsonAsync<dynamic>();
        string sasUrl;
        SasUrlResponse sasUrlResponse;
        try
        {
            sasUrlResponse = JsonSerializer.Deserialize<SasUrlResponse>(result);
            sasUrl = sasUrlResponse.SasToken;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            throw;
        }
        
        try
        {
            var blobHttpHeader = new BlobHttpHeaders { ContentType = "image/jpeg" };
            var blobClient = new BlobClient(new Uri(sasUrl));
            await blobClient.UploadAsync(fileStream, overwrite: false);
            await blobClient.SetHttpHeadersAsync(blobHttpHeader);
            return sasUrlResponse;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            throw;
        }
    }

    public async Task<Stream?> DownloadImageAsync(string fileName)
    {
        var groupId = "groupid1";
        var urlWithParams = $"{_blobServiceConfiguration.SasReadEndPoint}?selectedGroupId={groupId}&fileName={fileName}";
        var response = await _sasServiceClient.GetAsync(urlWithParams);
        if (!response.IsSuccessStatusCode) return null;
        var result = await response.Content.ReadFromJsonAsync<dynamic>();
        string sasUrl;
        SasUrlResponse sasUrlResponse;
        try
        {
            sasUrlResponse = JsonSerializer.Deserialize<SasUrlResponse>(result);
            sasUrl = sasUrlResponse.SasToken;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            throw;
        }
        
        try
        {
            //var blobHttpHeader = new BlobHttpHeaders { ContentType = "image/jpeg" };
            var blobClient = new BlobClient(new Uri(sasUrl));
            var stream = await blobClient.OpenReadAsync();
            //await blobClient.SetHttpHeadersAsync(blobHttpHeader);
            return stream;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            throw;
        }
    }
}