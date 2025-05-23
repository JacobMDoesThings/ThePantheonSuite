using Azure.Core;
using Azure.Identity;
using Azure.Storage.Files.DataLake;
using Azure.Storage.Files.DataLake.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ThePantheonSuite.ZeusOrchestrator;

public class AfterAccountCreatedFunction
{
    private readonly ILogger<AfterAccountCreatedFunction> _logger;

    public AfterAccountCreatedFunction(ILogger<AfterAccountCreatedFunction> logger)
    {
        _logger = logger;
    }

    [Function("AfterAccountCreatedFunction")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
    {
        // "AccountName": "devimgstorwestus01",
        // "MainContainerName": "main-container"
        const string dfsEndpoint = $"";
        //TokenCredential credential = new ClientSecretCredential(tenantId, clientId, clientSecret);
        //var serviceClient = new DataLakeServiceClient(new Uri(dfsEndpoint), new DefaultAzureCredential());
        var serviceClient = new DataLakeServiceClient(
            ""
        );
        var fileSystemClient = serviceClient.GetFileSystemClient("creationTest");

        // Create folder path dynamically named after user ID
        const string userId = "user123"; // Replace with actual user ID from your context
        var directoryClient = fileSystemClient.GetDirectoryClient($"users/{userId}");

        // Create folder recursively
        await directoryClient.CreateIfNotExistsAsync();

        // Set POSIX-style ACLs recursively
        var acl = new List<PathAccessControlItem>
        {
            new PathAccessControlItem(
                accessControlType: AccessControlType.User,
                permissions: RolePermissions.Read | RolePermissions.Write,
                entityId: userId),
            // new PathAccessControlItem(
            //     accessControlType: AccessControlType.Group,
            //     permissions: RolePermissions.Read | RolePermissions.Write | RolePermissions.Execute,
            //     entityId: "<elevated-group-id>"),
            new PathAccessControlItem(
                accessControlType: AccessControlType.Other,
                permissions: RolePermissions.None) // Deny others
        };

        try
        {
            // Set the access control list recursively
            var response = await directoryClient.SetAccessControlRecursiveAsync(acl);
            Console.WriteLine($"SetAccessControlRecursiveAsync completed. {response.Value}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
            // Handle exceptions appropriately
        }
        return new OkResult();
    }
}