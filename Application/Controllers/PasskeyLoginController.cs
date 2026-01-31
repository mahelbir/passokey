using Application.Common;
using Application.Models.PasskeyLogin;
using Application.Persistence.Client;
using Application.Persistence.User;
using Application.Services.Jwt;
using Microsoft.AspNetCore.Mvc;

namespace Application.Controllers;

public class PasskeyLoginController(
    ClientRepository clientRepository,
    JwtService jwtService
) : Controller
{
    [HttpGet("/auth/login/{clientId:guid}")]
    public async Task<IActionResult> Index(Guid clientId, string? redirectUri = "", string? state = "")
    {
        // Check client
        ClientEntity? client = null;
        if (clientId != Guid.Empty)
        {
            client = await clientRepository.GetById(clientId);
        }

        if (client == null)
        {
            return NotFound();
        }

        // Check if user is logged in
        var userIdString = HttpContext.Session.GetString($"authorizedClient.{clientId}.user");
        if (!string.IsNullOrEmpty(userIdString))
        {
            // Check expiration
            var expiresAtString = HttpContext.Session.GetString($"authorizedClient.{clientId}.expires");
            if (!string.IsNullOrEmpty(expiresAtString) && DateTime.TryParse(expiresAtString, out var expiresAt))
            {
                if (DateTime.UtcNow < expiresAt)
                {
                    // Create JWT token
                    var token = jwtService.CreateToken(client, new UserEntity
                    {
                        Id = Guid.Parse(userIdString)
                    });
                    var redirect = PasskeyHelper.GetRedirectUri(client, token, redirectUri, state);
                    if (!string.IsNullOrEmpty(redirect))
                    {
                        return Redirect(redirect);
                    }

                    return BadRequest("Invalid redirect URI");
                }
            }
        }

        var model = new PasskeyLoginViewModel
        {
            ClientId = clientId
        };

        return View(model);
    }

    [HttpGet("/auth/logout/{clientId:guid}")]
    public IActionResult Logout(Guid clientId, string redirect = "")
    {
        HttpContext.Session.Remove($"authorizedClient.{clientId}.user");
        HttpContext.Session.Remove($"authorizedClient.{clientId}.expires");
        if (string.IsNullOrEmpty(redirect))
        {
            redirect = $"/auth/login/{clientId}";
        }

        // Open Redirect
        return Redirect(redirect);
    }
}