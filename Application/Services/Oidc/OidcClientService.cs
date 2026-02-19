using Application.Persistence.Client;
using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Application.Services.Oidc;

public class OidcClientService(IOpenIddictApplicationManager applicationManager)
{
    public async Task CreateOrUpdateOidcApplication(ClientEntity client)
    {
        var clientId = client.Id.ToString();
        var existing = await applicationManager.FindByClientIdAsync(clientId);

        if (existing == null)
        {
            var descriptor = new OpenIddictApplicationDescriptor
            {
                ClientId = clientId,
                ClientSecret = client.SecretKey,
                DisplayName = client.Name,
                ClientType = ClientTypes.Confidential,
                ConsentType = ConsentTypes.Implicit,
                Permissions =
                {
                    Permissions.Endpoints.Authorization,
                    Permissions.Endpoints.Token,
                    Permissions.GrantTypes.AuthorizationCode,
                    Permissions.ResponseTypes.Code,
                    Permissions.Scopes.Email,
                    Permissions.Scopes.Profile,
                    Permissions.Prefixes.Scope + Scopes.OpenId
                }
            };

            foreach (var uri in client.RedirectUriList)
                if (Uri.TryCreate(uri, UriKind.Absolute, out var parsed))
                    descriptor.RedirectUris.Add(parsed);

            await applicationManager.CreateAsync(descriptor);
        }
        else
        {
            var descriptor = new OpenIddictApplicationDescriptor();
            await applicationManager.PopulateAsync(descriptor, existing);
            descriptor.DisplayName = client.Name;
            descriptor.ClientSecret = client.SecretKey;
            descriptor.RedirectUris.Clear();
            foreach (var uri in client.RedirectUriList)
                if (Uri.TryCreate(uri, UriKind.Absolute, out var parsed))
                    descriptor.RedirectUris.Add(parsed);

            await applicationManager.UpdateAsync(existing, descriptor);
        }
    }
}