using ThePantheonSuite.AthenaCore.Models;

namespace ThePantheonSuite.AthenaCore.Interfaces;

public interface IFileUploadService
{
    public Task<SasUrlResponse?> UploadImageAsync(Stream fileStream, bool isPublic);
}