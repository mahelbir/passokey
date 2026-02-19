using Application.Models.General;
using Application.Services.Jwt;
using Application.Services.Oidc;
using Microsoft.AspNetCore.Mvc;

namespace Application.Services;

public static class RegisterServices
{
    public static void AddAppServices(this IServiceCollection services, IConfiguration config)
    {
        services.AddSingleton<JwtService>();
        services.AddOidcServices(config);
    }

    public static void AddCoreServices(this IServiceCollection services, IConfiguration config)
    {
        // HttpContext
        services.AddHttpContextAccessor();
        services.AddControllersWithViews();

        // Session
        services.AddDistributedMemoryCache();
        services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromMinutes(config.GetValue<int>("Session:IdleTimeoutMinutes"));
            options.Cookie.Name = "SESSION_PK";
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.SameSite = SameSiteMode.Lax;
        });

        // Fido2
        var origins = config.GetSection("Fido2:Origins").Get<HashSet<string>>() ?? [];
        origins.Add(config.GetValue<string>("BaseUrl")!);
        services.AddFido2(options =>
        {
            options.ServerDomain = config["Fido2:ServerDomain"]!;
            options.ServerName = config["Fido2:ServerName"]!;
            options.Origins = origins;
            options.TimestampDriftTolerance = config.GetValue<int>("Fido2:TimestampDriftTolerance");
        });

        // Validators
        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var errors = context.ModelState
                    .Where(m => m.Value!.Errors.Any())
                    .SelectMany(kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage))
                    .ToList();

                var response = new ResponseModel
                {
                    StatusCode = 400,
                    Messages = errors
                };

                return new BadRequestObjectResult(response);
            };
        });
    }
}