namespace ThePantheonSuite.AthenaCore.Middleware;

using Microsoft.Extensions.Logging;

/// <summary>
/// Middleware component that validates Azure Entra B2C CIAM JWT tokens for HTTP requests.
/// </summary>
/// <remarks>
/// This middleware intercepts HTTP requests to verify authentication tokens issued by Azure Entra B2C.
/// It performs token validation against discovery endpoint keys and enforces audience/issuer constraints.
/// The middleware integrates with Azure Functions worker pipeline via <see cref="IFunctionsWorkerMiddleware"/>.
/// </remarks>
public class EntraB2CMiddleware(Configuration.ClientInfoConfiguration config, ILogger<EntraB2CMiddleware> logger) : IFunctionsWorkerMiddleware
{
    private readonly ILogger<EntraB2CMiddleware> _logger = logger;

    /// <summary>
    /// Processes HTTP requests by validating JWT tokens and populating user principal information.
    /// </summary>
    /// <param name="context">The function execution context containing HTTP request data.</param>
    /// <param name="next">Delegate to invoke subsequent middleware or function handler.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        var httpRequest = await context.GetHttpRequestDataAsync();

        if (httpRequest == null)
        {
            _logger.LogInformation("Non-HTTP request detected - bypassing token validation");
            await next(context);
            return;
        }

        if (!httpRequest.Headers.TryGetValues("Authorization", out var authHeader))
        {
            _logger.LogWarning("Missing Authorization header");
            var response = httpRequest.CreateResponse(HttpStatusCode.Unauthorized);
            await response.WriteStringAsync("Missing authorization header");
            return;
        }

        var token = authHeader.First().ToString().Replace("Bearer ", "");
        
        _logger.LogDebug("Extracted token from Authorization header");

        try
        {
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKeys = await GetSigningKeysAsync(),
                ValidAudience = config.ClientId,
                ValidIssuer = $"https://{config.TenantId}.ciamlogin.com/{config.TenantId}/v2.0",
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            _logger.LogDebug("Validating token with audience: {Audience} and issuer: {Issuer}", 
                validationParameters.ValidAudience, validationParameters.ValidIssuer);

            var principal = new JwtSecurityTokenHandler().ValidateToken(token, validationParameters, out _);
            
            _logger.LogInformation("Successfully validated token for user: {UserPrincipal}", principal.Identity?.Name);
            
            context.Items["UserPrincipal"] = principal;
        }
        catch (SecurityTokenInvalidSignatureException ex)
        {
            _logger.LogError(ex, "JWT token signature validation failed");
            var response = httpRequest.CreateResponse(HttpStatusCode.Unauthorized);
            await response.WriteStringAsync($"Invalid token signature");
            return;
        }
        catch (SecurityTokenExpiredException ex)
        {
            _logger.LogWarning(ex, "JWT token expired");
            var response = httpRequest.CreateResponse(HttpStatusCode.Unauthorized);
            await response.WriteStringAsync("Token expired");
            return;
        }
        catch (SecurityTokenInvalidAudienceException ex)
        {
            _logger.LogWarning(ex, "JWT token audience mismatch");
            var response = httpRequest.CreateResponse(HttpStatusCode.Forbidden);
            await response.WriteStringAsync("Invalid audience");
            return;
        }
        catch (SecurityTokenInvalidIssuerException ex)
        {
            _logger.LogWarning(ex, "JWT token issuer mismatch");
            var response = httpRequest.CreateResponse(HttpStatusCode.Forbidden);
            await response.WriteStringAsync("Invalid issuer");
            return;
        }
        catch (SecurityTokenException ex)
        {
            _logger.LogError(ex, "JWT token validation failed");
            var response = httpRequest.CreateResponse(HttpStatusCode.Unauthorized);
            await response.WriteStringAsync("Token validation failed");
            return;
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Unexpected error during token validation");
            var response = httpRequest.CreateResponse(HttpStatusCode.Forbidden);
            await response.WriteStringAsync("Access denied");
            return;
        }

        _logger.LogDebug("Proceeding to next middleware after successful token validation");
        await next(context);
    }

    /// <summary>
    /// Retrieves RSA signing keys from Azure Entra B2C discovery endpoint.
    /// </summary>
    /// <returns>Array of cryptographic keys used for token signature verification.</returns>
    /// <exception cref="Exception">Thrown when key retrieval fails or parsing encounters errors.</exception>
    private async Task<SecurityKey[]> GetSigningKeysAsync()
    {
        var httpClient = new HttpClient();
        var discoveryUrl =
            $"https://{config.TenantId}.ciamlogin.com/{config.TenantId}/discovery/v2.0/keys";

        _logger.LogDebug("Retrieving signing keys from discovery endpoint: {DiscoveryUrl}", discoveryUrl);

        var response = await httpClient.GetAsync(discoveryUrl);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Failed to retrieve signing keys from discovery endpoint. Status code: {StatusCode}", response.StatusCode);
            throw new Exception("Failed to retrieve signing keys");
        }

        var json = await response.Content.ReadAsStringAsync();
        
        _logger.LogDebug("Successfully retrieved signing keys JSON");

        var keysJson = JsonDocument.Parse(json);

        return keysJson.RootElement
            .GetProperty("keys")
            .EnumerateArray()
            .Select(k =>
            {
                try
                {
                    var modulusBase64 = k.GetProperty("n").GetString()
                        ?.Replace('-', '+')
                        .Replace('_', '/');

                    var exponentBase64 = k.GetProperty("e").GetString()
                        ?.Replace('-', '+')
                        .Replace('_', '/');

                    modulusBase64 = modulusBase64?.PadRight(modulusBase64.Length + (4 - modulusBase64.Length % 4) % 4, '=');
                    exponentBase64 = exponentBase64?.PadRight(exponentBase64.Length + (4 - exponentBase64.Length % 4) % 4, '=');

                    if (string.IsNullOrEmpty(modulusBase64) || string.IsNullOrEmpty(exponentBase64))
                        throw new Exception("Error parsing security key");
                    var modulus = Convert.FromBase64String(modulusBase64);
                    var exponent = Convert.FromBase64String(exponentBase64);

                    RSAParameters rsaParams = new()
                    {
                        Modulus = modulus,
                        Exponent = exponent
                    };

                    var rsaProvider = RSA.Create();
                    rsaProvider.ImportParameters(rsaParams);

                    return new RsaSecurityKey(rsaProvider);

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to parse security key");
                    throw;
                }
            })
            .ToArray<SecurityKey>();
    }
}
