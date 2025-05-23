using Azure.Storage.Files.DataLake;
using ThePantheonSuite.ZeusOrchestrator.Data;

namespace ThePantheonSuite.ZeusOrchestrator.Interfaces;

public interface IImageProcessingService
{
    public Task ProcessImageAsync(Uri blobUri);
    public Task ProcessImagePipelineAsync(BlobData blobData, DataLakeFileSystemClient fileClient);
    public DataLakeFileClient GetSourceFileClient(DataLakeFileSystemClient fileSystemClient, BlobData blobData);
    public Task ValidateMimeTypeAsync(DataLakeFileClient fileClient);
    public Task<MemoryStream> DownloadFileStreamAsync(DataLakeFileClient fileClient);
    public Task ValidateImageStructureAsync(MemoryStream stream);
    public Task<MemoryStream> GenerateThumbnailAsync(MemoryStream inputBlobStream);
    public DataLakeFileClient GetThumbnailFileClient(DataLakeFileSystemClient fileSystemClient, BlobData blobData);
    public Task UploadThumbnailAsync(DataLakeFileClient thumbFileClient, MemoryStream thumbnailBytes);
    public Task SetThumbnailMetadataAsync(DataLakeFileClient thumbFileClient);
}