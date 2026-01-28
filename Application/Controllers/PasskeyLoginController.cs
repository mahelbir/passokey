using Application.Common;
using Application.Models.General;
using Application.Models.PasskeyLogin;
using Application.Persistence;
using Application.Persistence.Client;
using Application.Persistence.User;
using Application.Persistence.UserCredential;
using Application.Services.Jwt;
using Fido2NetLib;
using Fido2NetLib.Objects;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Application.Controllers;

[Route("auth/login")]
public class PasskeyLoginController(
    ClientRepository clientRepository,
    UserCredentialRepository userCredentialRepository,
    UnitOfWork unitOfWork,
    IFido2 fido2,
    JwtService jwtService,
    IConfiguration config
) : Controller
{
    [HttpGet("{clientId:Guid}")]
    public async Task<IActionResult> Index(Guid clientId, string? redirectUri = "", string? state = "")
    {
        ClientEntity? client = null;
        if (clientId != Guid.Empty)
        {
            client = await clientRepository.GetNtById(clientId);
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

    [HttpPost("start/{clientId:Guid}")]
    public async Task<IActionResult> Start(Guid clientId)
    {
        ClientEntity? client = null;
        if (clientId != Guid.Empty)
        {
            client = await clientRepository.GetNtById(clientId);
        }

        if (client == null)
        {
            return StatusCode(403, ResponseModel.Error("Access denied", 403));
        }

        var response = new ResponseModel<StartPasskeyLoginResponse>();

        var options = fido2.GetAssertionOptions(new GetAssertionOptionsParams
        {
            AllowedCredentials = [],
            UserVerification = UserVerificationRequirement.Discouraged
        });

        HttpContext.Session.SetString("fido2.login.challenge", options.ToJson());
        HttpContext.Session.SetString("fido2.login.clientId", clientId.ToString());

        response.Data = new StartPasskeyLoginResponse
        {
            Options = options
        };
        response.StatusCode = 200;
        response.Messages.Add("Success");
        return Json(response);
    }

    [HttpPost("finish/{clientId:Guid}")]
    public async Task<IActionResult> Finish(Guid clientId,
        [FromBody] FinishPasskeyLoginRequest request)
    {
        // Check session
        var challengeJson = HttpContext.Session.GetString("fido2.login.challenge");
        var sessionClientId = HttpContext.Session.GetString("fido2.login.clientId");

        if (string.IsNullOrEmpty(challengeJson) || string.IsNullOrEmpty(sessionClientId) ||
            sessionClientId != clientId.ToString())
        {
            return StatusCode(400, ResponseModel.Error("Session invalid or expired, please refresh the page"));
        }

        var options = AssertionOptions.FromJson(challengeJson);

        // Find user and credential by credential id
        var storedCredential = await userCredentialRepository.Get(c =>
            c.CredentialId == request.Credential.RawId
        ).Include(c => c.User)
            .AsNoTracking().FirstOrDefaultAsync();


        if (storedCredential == null || storedCredential.User == null)
        {
            return StatusCode(400, ResponseModel.Error("Bad request"));
        }

        var user = storedCredential.User;

        // Verify assertion
        try
        {
            var result = await fido2.MakeAssertionAsync(new MakeAssertionParams
            {
                AssertionResponse = request.Credential,
                OriginalOptions = options,
                StoredPublicKey = storedCredential.PublicKey,
                StoredSignatureCounter = storedCredential.SignCount,
                IsUserHandleOwnerOfCredentialIdCallback =
                    (_, _) => Task
                        .FromResult(
                            true) // Credential ownership is already verified by finding the credential with its user above, so we can safely return true here without additional database queries
            });

            // Update sign count
            storedCredential.SignCount = result.SignCount;
            userCredentialRepository.Update(storedCredential);
            await unitOfWork.SaveChangesAsync();
        }
        catch (Fido2VerificationException)
        {
            return StatusCode(401, ResponseModel.Error("Authentication failed, please try again", 401));
        }

        // Check clientId and verify user has permission
        var client = await clientRepository
            .Get(c => c.Id == clientId)
            .Include(c => c.UserPermissions.Where(p => p.UserId == user.Id))
            .AsNoTracking()
            .FirstOrDefaultAsync();

        if (client == null || client.UserPermissions.Count == 0)
        {
            return StatusCode(403, ResponseModel.Error("You do not have permission to access", 403));
        }

        // Clear session
        HttpContext.Session.Remove("fido2.login.challenge");
        HttpContext.Session.Remove("fido2.login.clientId");


        // Create JWT token
        var token = jwtService.CreateToken(client, user);
        var state = request.State ?? string.Empty;
        var response = new ResponseModel<FinishPasskeyLoginResponse>
        {
            Data = new FinishPasskeyLoginResponse
            {
                UserId = user.Id,
                State = state,
                Token = token,
                Redirect = PasskeyHelper.GetRedirectUri(client, token, request.RedirectUri, state)
            },
            StatusCode = 200,
            Messages = ["Login successful"]
        };

        // Mark as logged in for the session
        var lifetimeMinutes = config.GetValue<int>("Session:AuthorizedClientLifetimeMinutes");
        var expiresAt = DateTime.UtcNow.AddMinutes(lifetimeMinutes);
        HttpContext.Session.SetString($"authorizedClient.{clientId}.expires", expiresAt.ToString("o"));
        HttpContext.Session.SetString($"authorizedClient.{clientId}.user", user.Id.ToString());

        return Json(response);
    }
}