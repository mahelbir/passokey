using Application.Persistence.Client;
using Application.Persistence.User;
using Application.Persistence.UserClientPermission;
using Application.Persistence.UserCredential;
using Microsoft.EntityFrameworkCore;

namespace Application.Persistence;

public static class RegisterPersistence
{
    public static void AddRepositories(this IServiceCollection services, IConfiguration config)
    {
        services.AddScoped<UserRepository>();
        services.AddScoped<ClientRepository>();
        services.AddScoped<UserCredentialRepository>();
        services.AddScoped<UserClientPermissionRepository>();
    }
    
    public static void AddPersistence(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<AppDbContext>(options => options.UseSqlite(
            config.GetConnectionString("DefaultConnection"),
            builder => builder.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)
        ));
        services.AddScoped(typeof(GenericRepository<>));
        services.AddScoped<UnitOfWork>();
    }
}