using System.Security.Claims;
using Application.Common;
using Application.Persistence.Client;
using Application.Persistence.User;
using Application.Persistence.UserClientPermission;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Application.Controllers;

public class OidcController(
    ClientRepository clientRepository,
    UserRepository userRepository,
    UserClientPermissionRepository permissionRepository,
    IConfiguration config
) : Controller
{
    [HttpGet("/oidc/authorize")]
    [HttpPost("/oidc/authorize")]
    public async Task<IActionResult> Authorize()
    {
        var oidcRequest = HttpContext.GetOpenIddictServerRequest()
                          ?? throw new InvalidOperationException("The OIDC request cannot be retrieved.");

        // Find the Passokey ClientEntity by Id (used as OIDC client_id)
        if (!Guid.TryParse(oidcRequest.ClientId, out var parsedClientId))
            return Forbid(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

        var client = await clientRepository
            .Query(c => c.Id == parsedClientId)
            .FirstOrDefaultAsync();

        if (client == null) return Forbid(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

        // Check if user has active session for this client
        var userId = HttpContext.Session.GetAuthorizedClientUserId(client);

        if (userId == null)
        {
            // Store OIDC request parameters in session
            HttpContext.Session.SetString("oidc.client_id", oidcRequest.ClientId ?? "");
            HttpContext.Session.SetString("oidc.redirect_uri", oidcRequest.RedirectUri ?? "");
            HttpContext.Session.SetString("oidc.response_type", oidcRequest.ResponseType ?? "");
            HttpContext.Session.SetString("oidc.scope", oidcRequest.Scope ?? "");
            HttpContext.Session.SetString("oidc.state", oidcRequest.State ?? "");
            HttpContext.Session.SetString("oidc.code_challenge", oidcRequest.CodeChallenge ?? "");
            HttpContext.Session.SetString("oidc.code_challenge_method", oidcRequest.CodeChallengeMethod ?? "");
            HttpContext.Session.SetString("oidc.nonce", oidcRequest.Nonce ?? "");

            var returnPath = Uri.EscapeDataString("/oidc/authorize/callback");
            return Redirect($"/auth/login/{client.Id}?returnPath={returnPath}");
        }

        // User is logged in, issue authorization code
        return await IssueAuthorizationCode(client, userId.Value, oidcRequest);
    }

    [HttpGet("/oidc/authorize/callback")]
    public IActionResult Callback()
    {
        // Retrieve stored OIDC request from session
        var oidcClientId = HttpContext.Session.GetString("oidc.client_id");
        if (string.IsNullOrEmpty(oidcClientId)) return BadRequest("No pending OIDC request");

        var redirectUri = HttpContext.Session.GetString("oidc.redirect_uri") ?? "";
        var responseType = HttpContext.Session.GetString("oidc.response_type") ?? "";
        var scope = HttpContext.Session.GetString("oidc.scope") ?? "";
        var state = HttpContext.Session.GetString("oidc.state") ?? "";
        var codeChallenge = HttpContext.Session.GetString("oidc.code_challenge") ?? "";
        var codeChallengeMethod = HttpContext.Session.GetString("oidc.code_challenge_method") ?? "";
        var nonce = HttpContext.Session.GetString("oidc.nonce") ?? "";

        // Clear OIDC session data
        ClearOidcSession();

        // Redirect back to /oidc/authorize with original params
        var query = new Dictionary<string, string?>
        {
            ["client_id"] = oidcClientId,
            ["redirect_uri"] = redirectUri,
            ["response_type"] = responseType,
            ["scope"] = scope
        };

        if (!string.IsNullOrEmpty(state)) query["state"] = state;
        if (!string.IsNullOrEmpty(codeChallenge)) query["code_challenge"] = codeChallenge;
        if (!string.IsNullOrEmpty(codeChallengeMethod)) query["code_challenge_method"] = codeChallengeMethod;
        if (!string.IsNullOrEmpty(nonce)) query["nonce"] = nonce;

        var authorizeUrl = QueryHelpers.AddQueryString("/oidc/authorize", query);
        return Redirect(authorizeUrl);
    }

    private async Task<IActionResult> IssueAuthorizationCode(
        ClientEntity client, Guid userId, OpenIddictRequest oidcRequest)
    {
        // Check UserClientPermission
        var hasPermission = await permissionRepository
            .Query(p => p.UserId == userId && p.ClientId == client.Id)
            .AnyAsync();

        if (!hasPermission) return Forbid(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

        // Load user for claims
        var user = await userRepository.GetById(userId);
        if (user == null) return BadRequest("User not found");

        // Build claims principal
        var baseUrl = config["BaseUrl"]!;
        var host = new Uri(baseUrl).Host;
        var email = $"{user.Username}@{host}"; // Construct a unique email

        var claims = new List<Claim>
        {
            new(Claims.Subject, userId.ToString()),
            new(Claims.Email, email),
            new(Claims.Name, user.Username)
        };

        var identity = new ClaimsIdentity(claims,
            OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
            Claims.Name,
            Claims.Role);

        // Set destinations for each claim
        foreach (var claim in identity.Claims)
        {
            var destinations = claim.Type switch
            {
                Claims.Subject => new[] { Destinations.AccessToken, Destinations.IdentityToken },
                Claims.Email => new[] { Destinations.AccessToken, Destinations.IdentityToken },
                Claims.Name => new[] { Destinations.AccessToken, Destinations.IdentityToken },
                _ => new[] { Destinations.AccessToken }
            };
            claim.SetDestinations(destinations);
        }

        var principal = new ClaimsPrincipal(identity);
        principal.SetScopes(oidcRequest.GetScopes());

        return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    private void ClearOidcSession()
    {
        HttpContext.Session.Remove("oidc.client_id");
        HttpContext.Session.Remove("oidc.redirect_uri");
        HttpContext.Session.Remove("oidc.response_type");
        HttpContext.Session.Remove("oidc.scope");
        HttpContext.Session.Remove("oidc.state");
        HttpContext.Session.Remove("oidc.code_challenge");
        HttpContext.Session.Remove("oidc.code_challenge_method");
        HttpContext.Session.Remove("oidc.nonce");
    }
}