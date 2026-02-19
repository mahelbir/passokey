using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using OpenIddict.Validation.AspNetCore;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Application.Controllers.Api;

[ApiController]
public class OidcApiController : ControllerBase
{
    [HttpPost("/oidc/token")]
    [Produces("application/json")]
    public async Task<IActionResult> Token()
    {
        var request = HttpContext.GetOpenIddictServerRequest()
                      ?? throw new InvalidOperationException("The OIDC request cannot be retrieved.");

        if (request.IsAuthorizationCodeGrantType())
        {
            var result = await HttpContext.AuthenticateAsync(
                OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

            if (!result.Succeeded) return Forbid(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

            return SignIn(result.Principal!, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        return BadRequest("Unsupported grant type");
    }

    [HttpGet("/oidc/userinfo")]
    [HttpPost("/oidc/userinfo")]
    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    public IActionResult UserInfo()
    {
        var sub = User.GetClaim(Claims.Subject);
        var email = User.GetClaim(Claims.Email);
        var name = User.GetClaim(Claims.Name);

        if (string.IsNullOrEmpty(sub)) return Forbid(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);

        return Ok(new
        {
            sub,
            email,
            name
        });
    }
}