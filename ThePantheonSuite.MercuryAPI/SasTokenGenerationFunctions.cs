using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker.Http;
using ThePantheonSuite.AthenaCore.Interfaces;
using ThePantheonSuite.AthenaCore.SasService;

namespace ThePantheonSuite.MercuryAPI;

public class SasTokenGenerationFunctions
{
    private readonly ILogger<SasTokenGenerationFunctions> _logger;
    private readonly ISasGeneratorService _sasGeneratorService;

    public SasTokenGenerationFunctions(ILogger<SasTokenGenerationFunctions> logger, 
        ISasGeneratorService sasGeneratorService)
    {
        _logger = logger;
        _sasGeneratorService = sasGeneratorService;
    }

    // [Authorize]
    [Function("GenerateWriteSasToken")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous,
        "get", "post")] HttpRequestData req)
    {
        // 500 instead of 401 because claims principal is handled in middleware. If a user has no claims
        // principal at this point, this means middleware failed / InternalServerError. Returning anything but 500
        // may mislead user otherwise.
        if (!req.FunctionContext.Items.TryGetValue("UserPrincipal", out var principalObj)) return new StatusCodeResult(500);
        var principal = (ClaimsPrincipal)principalObj;
        var body = JsonSerializer.Deserialize<SasGenerationRequestModel>(await req.ReadAsStringAsync() ?? string.Empty);
        if (body is null) return new BadRequestResult();
        var token = await _sasGeneratorService.GenerateWriteSasTokenAsync(principal, body);
        return new OkObjectResult(token);
    }
}