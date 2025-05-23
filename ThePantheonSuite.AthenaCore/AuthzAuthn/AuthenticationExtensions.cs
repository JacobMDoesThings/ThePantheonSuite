using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace ThePantheonSuite.AthenaCore.AuthzAuthn;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;

public static class AuthenticationExtensions
{
    public static void AddCustomAzAdAuthentication(this IServiceCollection services, 
        IConfiguration configuration,
        string clientSectionName)
    {
        var configSection = configuration.GetRequiredSection(clientSectionName);
        services.Configure<ClientInfoConfiguration>(configSection);
        var clientInfoConfig = configSection.Get<ClientInfoConfiguration>();
        if(string.IsNullOrEmpty(clientInfoConfig?.TenantId))
            throw new InvalidOperationException("Client Tenant Id is required.");
        
        if (string.IsNullOrEmpty(clientInfoConfig?.TenantName))
            throw new InvalidOperationException("Client Tenant Name is required.");
        
        if (string.IsNullOrEmpty(clientInfoConfig?.ClientId))
            throw new InvalidOperationException("Client Id is required.");
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKeys = GetAzureAdSigningKeys(clientInfoConfig.TenantName),
                    ValidateIssuer = true,
                    ValidIssuer = $"https://{clientInfoConfig.TenantId}.ciamlogin.com/{clientInfoConfig.TenantId}/v2.0",
                    ValidateAudience = true,
                    ValidAudience = clientInfoConfig.ClientId,
                    ValidateLifetime = true
                };
            });
    }

    private static IEnumerable<SecurityKey> GetAzureAdSigningKeys(string tenantName)
    {
        var jwksUri =
            $"https://{tenantName}.ciamlogin.com/{tenantName}.onmicrosoft.com/oauth2/v2.0/authorize?p=signin-signup";
        var httpClient = new HttpClient();
        var response = httpClient.GetAsync(jwksUri).Result;
        var json = response.Content.ReadAsStringAsync().Result;

        // Parse JSON Web Key Set (JWKS) and extract keys
        var jwks = JsonSerializer.Deserialize<JsonWebKeySet>(json);
        return jwks.Keys;
    }
}
