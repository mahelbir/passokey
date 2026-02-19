using Application.Persistence;
using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;
using OpenIddict.Server;

namespace Application.Services.Oidc;

public static class RegisterOidc
{
    public static void AddOidcServices(this IServiceCollection services, IConfiguration config)
    {
        services.AddSingleton<SigningKeyService>();
        services.AddScoped<OidcClientService>();
        services.AddSingleton<IConfigureOptions<OpenIddictServerOptions>, ConfigureOpenIddictServerOptions>();

        services.AddOpenIddict()
            .AddCore(options =>
            {
                options.UseEntityFrameworkCore()
                    .UseDbContext<AppDbContext>();
            })
            .AddServer(options =>
            {
                options.AllowAuthorizationCodeFlow();

                options.SetIssuer(new Uri(config["BaseUrl"]!));

                options.SetAuthorizationEndpointUris("/oidc/authorize")
                    .SetTokenEndpointUris("/oidc/token")
                    .SetUserInfoEndpointUris("/oidc/userinfo");

                options.RegisterScopes(
                    OpenIddictConstants.Scopes.OpenId,
                    OpenIddictConstants.Scopes.Email,
                    OpenIddictConstants.Scopes.Profile
                );

                options.UseAspNetCore()
                    .EnableAuthorizationEndpointPassthrough()
                    .EnableTokenEndpointPassthrough()
                    .EnableUserInfoEndpointPassthrough()
                    .DisableTransportSecurityRequirement();

                options.AddDevelopmentEncryptionCertificate();
                options.DisableAccessTokenEncryption();
            })
            .AddValidation(options =>
            {
                options.UseLocalServer();
                options.UseAspNetCore();
            });
    }
}