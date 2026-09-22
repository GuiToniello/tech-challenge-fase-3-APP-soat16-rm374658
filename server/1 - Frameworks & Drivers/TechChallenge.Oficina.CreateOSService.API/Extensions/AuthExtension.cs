using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;

namespace TechChallenge.Oficina.CreateOSService.API.Extensions;

public static class AuthExtension
{
    public static IServiceCollection AddAuthConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        var authSettingsSection = configuration.GetSection("AuthSettings");

        var authority = authSettingsSection["Authority"]
            ?? throw new InvalidOperationException("AuthSettings:Authority é obrigatório.");

        var audience = authSettingsSection["Audience"]
            ?? throw new InvalidOperationException("AuthSettings:Audience é obrigatório.");

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = $"https://{authority}";
                options.Audience = audience;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true
                };
            });

        services.AddAuthorizationBuilder()
            .SetFallbackPolicy(new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build());

        return services;
    }
}
