using Application.Common;
using Application.Persistence.Client;
using Application.Services.Oidc;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;

namespace Application.Persistence;

public static class AppDbSeed
{
    public static async Task SeedDatabase(this WebApplication app, IConfiguration config)
    {
        using var scope = app.Services.CreateScope();
        var serviceProvider = scope.ServiceProvider;
        var unitOfWork = serviceProvider.GetRequiredService<UnitOfWork>();
        var clientRepository = serviceProvider.GetRequiredService<ClientRepository>();

        await Clients(clientRepository, config);
        await unitOfWork.SaveChangesAsync();

        await OidcScopes(serviceProvider);
        await OidcApplications(serviceProvider, clientRepository);
    }

    private static async Task OidcScopes(IServiceProvider serviceProvider)
    {
        var scopeManager = serviceProvider.GetRequiredService<IOpenIddictScopeManager>();
        foreach (var scopeName in new[] { "openid", "email", "profile" })
            if (await scopeManager.FindByNameAsync(scopeName) == null)
                await scopeManager.CreateAsync(new OpenIddictScopeDescriptor { Name = scopeName });
    }

    private static async Task OidcApplications(IServiceProvider serviceProvider, ClientRepository clientRepository)
    {
        var oidcClientService = serviceProvider.GetRequiredService<OidcClientService>();
        var clients = await clientRepository.Query(c => true).ToListAsync();
        foreach (var client in clients) await oidcClientService.CreateOrUpdateOidcApplication(client);
    }

    private static async Task Clients(ClientRepository clientRepository, IConfiguration config)
    {
        if (!await clientRepository.Query(c => c.IsAdmin).AnyAsync())
        {
            var client = new ClientEntity
            {
                Name = "Admin",
                SecretKey = PasskeyHelper.GenerateClientSecretKey(),
                RedirectUriList = [config["BaseUrl"] + "/admin/login"],
                IsRegistrationEnabled = true,
                IsAdmin = true
            };
            await clientRepository.Create(client);
        }
    }
}