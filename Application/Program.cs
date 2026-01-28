using Application.Middlewares;
using Application.Persistence;
using Application.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddRepositories(builder.Configuration);
builder.Services.AddCoreServices(builder.Configuration);
builder.Services.AddAppServices(builder.Configuration);

var app = builder.Build();
app.SeedDatabase(builder.Configuration).Wait();
app.UseMiddlewares(builder.Configuration, app.Environment);
app.Run();