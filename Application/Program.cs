using Application.Middlewares;
using Application.Persistence;
using Application.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("App_Data/appsettings.json", optional: true, reloadOnChange: true);
builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddRepositories(builder.Configuration);
builder.Services.AddCoreServices(builder.Configuration);
builder.Services.AddAppServices(builder.Configuration);

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}
app.SeedDatabase(builder.Configuration).Wait();
app.UseMiddlewares(builder.Configuration, app.Environment);
app.Run();