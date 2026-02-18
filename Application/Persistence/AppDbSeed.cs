using Application.Common;
using Application.Persistence.Client;
using Microsoft.EntityFrameworkCore;

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