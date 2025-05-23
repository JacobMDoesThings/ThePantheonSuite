using ThePantheonSuite.AthenaCore.SasService;

namespace ThePantheonSuite.AthenaCore.Interfaces;

public interface IFileUploadService
{
    public Task<SasUrlResponse?> UploadImageAsync(Stream fileStream, bool isPublic);
}