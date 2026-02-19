using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Server;

namespace Application.Services.Oidc;

public class ConfigureOpenIddictServerOptions(SigningKeyService signingKeyService)
    : IConfigureOptions<OpenIddictServerOptions>
{
    public void Configure(OpenIddictServerOptions options)
    {
        options.SigningCredentials.Add(new SigningCredentials(
            signingKeyService.GetSigningKey(),
            SecurityAlgorithms.RsaSha256));
    }
}