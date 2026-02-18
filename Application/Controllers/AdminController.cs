using Application.Common;
using Application.Middlewares;
using Application.Models.General;
using Application.Persistence.Client;
using Application.Services.Jwt;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Application.Controllers;

public class AdminController(
    ClientRepository clientRepository,
    JwtService jwtService,
    IConfiguration config
) : Controller
{
    [AdminAuthorize]
    public IActionResult Index()
    {
        return RedirectToAction("Index", "Clients");
    }

    public async Task<IActionResult> Auth(string state = "/admin")
    {
        var result = await clientRepository
            .Query(c => c.IsAdmin)
            .Select(c => new { c.Id, c.RedirectUriList, PermissionCount = c.UserPermissions.Count })
            .FirstOrDefaultAsync();

        if (result == null)
        {
            return BadRequest("No admin client");
        }

        if (result.PermissionCount == 0)
        {
            return Redirect($"/auth/registration/{result.Id}?username=admin");
        }

        if (!HttpContext.IsAdminAuthorized())
        {
            var redirectUri = result.RedirectUriList.First();
            return Redirect(
                $"/auth/login/{result.Id}?redirectUri={Uri.EscapeDataString(redirectUri)}&state={Uri.EscapeDataString(state)}");
        }

        return Redirect(state);
    }

    public async Task<IActionResult> Login(Guid clientId, string token, string state = "/admin")
    {
        if (string.IsNullOrEmpty(token))
        {
            return StatusCode(400, ResponseModel.Error("Missing token"));
        }

        // Check admin client
        ClientEntity? client = null;
        if (clientId != Guid.Empty)
        {
            client = await clientRepository.Query(c =>
                c.Id == clientId &&
                c.IsAdmin == true
            ).FirstOrDefaultAsync();
        }

        if (client == null)
        {
            return StatusCode(403, ResponseModel.Error("Access denied", 403));
        }

        // Validate token
        var principal = jwtService.ValidateToken(client, token, config.GetValue<string>("BaseUrl"));
        if (principal == null)
        {
            return StatusCode(401, ResponseModel.Error("Authentication failed", 401));
        }

        // Create admin session
        var userIdString = principal.FindFirst("userId")?.Value!;
        var lifetimeMinutes = config.GetValue<int>("Session:AdminSessionLifetimeMinutes");
        var expiresAt = DateTime.UtcNow.AddMinutes(lifetimeMinutes);
        HttpContext.Session.SetString("admin.client", client.Id.ToString());
        HttpContext.Session.SetString("admin.user", userIdString);
        HttpContext.Session.SetString("admin.expires", expiresAt.ToString("o"));
        HttpContext.Session.SetString("admin.userAgent", Request.Headers.UserAgent.ToString());

        return Redirect(state);
    }

    public async Task<IActionResult> Logout()
    {
        var clientId = HttpContext.Session.GetAdminClientId();
        var result = await clientRepository
            .Query(c => c.IsAdmin && c.Id == clientId)
            .Select(c => new { c.Id, c.RedirectUriList })
            .FirstOrDefaultAsync();
        HttpContext.Session.ClearAdminSession();
        return Redirect(
            $"/auth/logout/{clientId}?redirectUri={Uri.EscapeDataString(result?.RedirectUriList.FirstOrDefault() ?? "")}");
    }
}