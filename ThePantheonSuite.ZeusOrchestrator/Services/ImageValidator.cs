using SixLabors.ImageSharp;
using ThePantheonSuite.ZeusOrchestrator.Interfaces;

namespace ThePantheonSuite.ZeusOrchestrator.Services;

public class ImageValidator : IImageValidator
{
    public async Task Validate(MemoryStream stream)
    {
        try
        {
            await Image.LoadAsync(stream); // Throws exceptions for invalid images
        }
        catch (Exception ex)
        {
            throw new ImageValidationException("Invalid image structure detected", ex);
        }
    }
}

public class ImageValidationException(string message, Exception innerException) : Exception(message, innerException);
