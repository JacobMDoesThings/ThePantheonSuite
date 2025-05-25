using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.IdentityModel.Tokens;

namespace ThePantheonSuite.AthenaCore.Middleware
{
    public class EntraB2CMiddleware(Configuration.ClientInfoConfiguration config) : IFunctionsWorkerMiddleware
    {
        public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
        {
            var httpRequest = await context.GetHttpRequestDataAsync();

            if (httpRequest == null)
            {
                // Not HTTP request — skip middleware
                await next(context);
                return;
            }

            if (!httpRequest.Headers.TryGetValues("Authorization", out var authHeader))
            {
                // No Authorization header → Unauthorized
                var response = httpRequest.CreateResponse(HttpStatusCode.Unauthorized);
                await response.WriteStringAsync("Missing authorization header");
                return;
            }

            var token = authHeader.First().ToString().Replace("Bearer ", "");

            try
            {
                // Validate token using Entra B2C discovery endpoint keys
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

                var principal = new JwtSecurityTokenHandler().ValidateToken(token, validationParameters, out _);
                context.Items["UserPrincipal"] = principal;
            }
            catch (SecurityTokenInvalidSignatureException ex)
            {
                // Invalid token signature → Unauthorized
                var response = httpRequest.CreateResponse(HttpStatusCode.Unauthorized);
                await response.WriteStringAsync($"Invalid token signature: {ex.Message}");
                return;
            }
            catch (SecurityTokenExpiredException ex)
            {
                // Expired token → Unauthorized
                var response = httpRequest.CreateResponse(HttpStatusCode.Unauthorized);
                await response.WriteStringAsync($"Token expired: {ex.Message}");
                return;
            }
            catch (SecurityTokenInvalidAudienceException ex)
            {
                // Invalid audience → Forbidden
                var response = httpRequest.CreateResponse(HttpStatusCode.Forbidden);
                await response.WriteStringAsync($"Invalid audience: {ex.Message}");
                return;
            }
            catch (SecurityTokenInvalidIssuerException ex)
            {
                // Invalid issuer → Forbidden
                var response = httpRequest.CreateResponse(HttpStatusCode.Forbidden);
                await response.WriteStringAsync($"Invalid issuer: {ex.Message}");
                return;
            }
            catch (SecurityTokenException ex)
            {
                // Other token validation errors → Unauthorized
                var response = httpRequest.CreateResponse(HttpStatusCode.Unauthorized);
                await response.WriteStringAsync($"Token validation failed: {ex.Message}");
                return;
            }
            catch (Exception ex)
            {
                // Fallback error handling → Forbidden
                var response = httpRequest.CreateResponse(HttpStatusCode.Forbidden);
                await response.WriteStringAsync($"Access denied: {ex.Message}");
                return;
            }

            // Proceed to next middleware or function handler
            await next(context);
        }

        private async Task<SecurityKey[]> GetSigningKeysAsync()
        {
            var httpClient = new HttpClient();
            var discoveryUrl =
                $"https://{config.TenantId}.ciamlogin.com/{config.TenantId}/discovery/v2.0/keys";

            var response = await httpClient.GetAsync(discoveryUrl);

            if (!response.IsSuccessStatusCode)
                throw new Exception("Failed to retrieve signing keys");

            var json = await response.Content.ReadAsStringAsync();
            var keysJson = JsonDocument.Parse(json);

            return keysJson.RootElement
                .GetProperty("keys")
                .EnumerateArray()
                .Select(k =>
                {
                    var modulusBase64 = k.GetProperty("n").GetString()
                        ?.Replace('-', '+')
                        .Replace('_', '/');

                    var exponentBase64 = k.GetProperty("e").GetString()
                        ?.Replace('-', '+')
                        .Replace('_', '/');

                    modulusBase64 = modulusBase64?.PadRight(modulusBase64.Length + (4 - modulusBase64.Length % 4) % 4, '=');
                    exponentBase64 = exponentBase64?.PadRight(exponentBase64.Length + (4 - exponentBase64.Length % 4) % 4, '=');

                    if (!string.IsNullOrEmpty(modulusBase64) && !string.IsNullOrEmpty(exponentBase64))
                    {
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

                    throw new Exception("Error parsing security key");
                })
                .ToArray<SecurityKey>();
        }
    }
}
