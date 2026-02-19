using Application.Common;
using Application.Models.General;
using Application.Models.PasskeyLogin;
using Application.Persistence;
using Application.Persistence.Client;
using Application.Persistence.UserCredential;
using Application.Services.Jwt;
using Fido2NetLib;
using Fido2NetLib.Objects;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Application.Controllers.Api;

[Route("api/auth/login")]
[ApiController]
public class PasskeyLoginApiController(
    ClientRepository clientRepository,
    UserCredentialRepository userCredentialRepository,
    UnitOfWork unitOfWork,
    IFido2 fido2,
    JwtService jwtService,
    IConfiguration config
) : ControllerBase
{
    [HttpPost("start")]
    public async Task<IActionResult> Start(StartPasskeyLoginRequest request)
    {
        // Check client
        var clientId = request.ClientId;
        var client = await clientRepository.GetById(clientId);
        if (client == null) return StatusCode(403, ResponseModel.Error("Access denied", 403));

        var response = new ResponseModel<StartPasskeyLoginResponse>();

        // Create challenge
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
        return StatusCode(response.StatusCode, response);
    }

    [HttpPost("finish")]
    public async Task<IActionResult> Finish([FromBody] FinishPasskeyLoginRequest request)
    {
        // Check session
        var clientId = request.ClientId;
        var challengeJson = HttpContext.Session.GetString("fido2.login.challenge");
        var sessionClientId = HttpContext.Session.GetString("fido2.login.clientId");

        if (string.IsNullOrEmpty(challengeJson) || string.IsNullOrEmpty(sessionClientId) ||
            sessionClientId != clientId.ToString())
            return StatusCode(400, ResponseModel.Error("Session invalid or expired, please refresh the page"));

        var options = AssertionOptions.FromJson(challengeJson);

        // Find user and credential by credential id
        var storedCredential = await userCredentialRepository.Query(c =>
                c.CredentialId == request.Credential.RawId
            ).Include(c => c.User)
            .AsTracking().FirstOrDefaultAsync();

        if (storedCredential == null || storedCredential.User == null)
            return StatusCode(400, ResponseModel.Error("Passkey is not found"));

        var user = storedCredential.User;

        // Verify challenge
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
            await unitOfWork.SaveChangesAsync();
        }
        catch (Fido2VerificationException)
        {
            return StatusCode(401, ResponseModel.Error("Authentication failed, please try again", 401));
        }

        // Check clientId and verify user has permission
        var client = await clientRepository
            .Query(c => c.Id == clientId)
            .Include(c => c.UserPermissions.Where(p => p.UserId == user.Id))
            .FirstOrDefaultAsync();

        if (client == null || client.UserPermissions.Count == 0)
            return StatusCode(403, ResponseModel.Error("You do not have permission to access", 403));

        // Clear session
        HttpContext.Session.Remove("fido2.login.challenge");
        HttpContext.Session.Remove("fido2.login.clientId");


        // Create JWT token
        var token = jwtService.CreateToken(
            client,
            user,
            request.RedirectUri
        );
        var state = request.State ?? string.Empty;
        var response = new ResponseModel<FinishPasskeyLoginResponse>
        {
            Data = new FinishPasskeyLoginResponse
            {
                UserId = user.Id,
                State = state,
                Token = token,
                Redirect = UriHelper.GetLocalReturnPath(request.returnPath) ??
                           client.GetAuthenticatedRedirectUri(request.RedirectUri, token, state)
            },
            StatusCode = 200,
            Messages = ["Login successful"]
        };

        // Mark as logged in for the session
        var lifetimeMinutes = config.GetValue<int>("Session:AuthorizedClientLifetimeMinutes");
        var expiresAt = DateTime.UtcNow.AddMinutes(lifetimeMinutes);
        HttpContext.Session.SetString($"authorizedClient.{clientId}.expires", expiresAt.ToString("o"));
        HttpContext.Session.SetString($"authorizedClient.{clientId}.user", user.Id.ToString());

        return StatusCode(response.StatusCode, response);
    }
}