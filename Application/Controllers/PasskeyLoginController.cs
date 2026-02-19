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
    public async Task<IActionResult> Index(Guid clientId, string? redirectUri = null, string? state = null,
        string? returnPath = null)
    {
        // Check client
        ClientEntity? client = null;
        if (clientId != Guid.Empty) client = await clientRepository.GetById(clientId);

        if (client == null) return NotFound();

        // Check if user is logged in
        var userId = HttpContext.Session.GetAuthorizedClientUserId(client);
        if (userId != null)
        {
            returnPath = UriHelper.GetLocalReturnPath(returnPath);
            if (returnPath != null) return Redirect(returnPath);

            // Create JWT token
            var token = jwtService.CreateToken(
                client,
                new UserEntity { Id = userId.Value },
                redirectUri
            );
            var authenticatedRedirectUri = client.GetAuthenticatedRedirectUri(redirectUri, token, state);
            if (!string.IsNullOrEmpty(authenticatedRedirectUri)) return Redirect(authenticatedRedirectUri);

            return BadRequest("Invalid redirect URI");
        }

        var model = new PasskeyLoginViewModel
        {
            ClientId = clientId
        };

        return View(model);
    }

    [HttpGet("/auth/logout/{clientId:guid}")]
    public async Task<IActionResult> Logout(Guid clientId, string? redirectUri)
    {
        // Check client
        ClientEntity? client = null;
        if (clientId != Guid.Empty) client = await clientRepository.GetById(clientId);

        if (client == null) return NotFound();

        HttpContext.Session.ClearAuthorizedClientSession(client);
        var resolvedUri = client.GetResolvedRedirectUri(redirectUri) ?? "";
        var returnPath = $"/auth/login/{clientId}?redirectUri={Uri.EscapeDataString(resolvedUri)}";
        return Redirect(returnPath);
    }
}