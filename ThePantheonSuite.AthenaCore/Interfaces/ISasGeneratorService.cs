using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using ThePantheonSuite.AthenaCore.SasService;

namespace ThePantheonSuite.AthenaCore.Interfaces;

public interface ISasGeneratorService
{
    public Task<SasUrlResponse> GenerateWriteSasTokenAsync(
        ClaimsPrincipal principal, SasGenerationRequestModel sasGenerationRequestModel);
    public Task<IEnumerable<SasUrlResponse>> BulkGenerateWriteSasTokenAsync(SasGenerationRequestModel sasGenerationRequestModel, int count);
    
    public Task<SasUrlResponse> GenerateReadSasTokenAsync(SasGenerationRequestModel sasGenerationRequestModel, string imageName);
}