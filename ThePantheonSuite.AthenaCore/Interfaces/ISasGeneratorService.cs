namespace ThePantheonSuite.AthenaCore.Interfaces;

public interface ISasGeneratorService
{
    public Task<SasUrlResponse> GenerateWriteSasTokenAsync(
        ClaimsPrincipal principal, SasGenerationRequest sasGenerationRequest);
    public Task<IEnumerable<SasUrlResponse>> BulkGenerateWriteSasTokenAsync(SasGenerationRequest sasGenerationRequest, int count);
    
    public Task<SasUrlResponse> GenerateReadSasTokenAsync(SasGenerationRequest sasGenerationRequest, string imageName);
}