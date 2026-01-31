using Application.Common;
using Application.Models.General;
using Application.Models.PasskeyRegistration;
using Application.Persistence;
using Application.Persistence.Client;
using Application.Persistence.User;
using Application.Persistence.UserClientPermission;
using Application.Persistence.UserCredential;
using Fido2NetLib;
using Fido2NetLib.Objects;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Application.Controllers;

[Route("api/auth/registration")]
[ApiController]
public class PasskeyRegistrationApiController(
    ClientRepository clientRepository,
    UserRepository userRepository,
    UserCredentialRepository userCredentialRepository,
    UnitOfWork unitOfWork,
    IFido2 fido2) : ControllerBase
{
    [HttpPost("start")]
    public async Task<IActionResult> Start([FromBody] StartPasskeyRegistrationRequest request)
    {
        // Check client
        var clientId = request.ClientId;
        var client = await clientRepository.GetByRegistrationEnabled(clientId);
        if (client == null)
        {
            return StatusCode(403, ResponseModel.Error("Access denied", 403));
        }

        var response = new ResponseModel<StartPasskeyRegistrationResponse>();
        var username = PasskeyHelper.GetUsername(request.Username);
        var userId = Guid.NewGuid();

        // Check user existence
        if (await userRepository.IsExists(userId, username))
        {
            return StatusCode(400, ResponseModel.Error("User already exists"));
        }

        // Create challenge
        var user = new Fido2User
        {
            Id = userId.ToByteArray(),
            Name = username,
            DisplayName = username
        };
        var requestNewCredentialParams = new RequestNewCredentialParams
        {
            User = user,
            ExcludeCredentials = new List<PublicKeyCredentialDescriptor>(),
            AuthenticatorSelection = new AuthenticatorSelection
            {
                AuthenticatorAttachment = null,
                ResidentKey = ResidentKeyRequirement.Required,
                UserVerification = UserVerificationRequirement.Discouraged
            },
            AttestationPreference = AttestationConveyancePreference.None
        };
        var options = fido2.RequestNewCredential(requestNewCredentialParams);

        HttpContext.Session.SetString("fido2.registration.challenge", options.ToJson());
        HttpContext.Session.SetString("fido2.registration.clientId", clientId.ToString());

        response.Data = new StartPasskeyRegistrationResponse
        {
            Options = options
        };
        response.StatusCode = 200;
        response.Messages.Add("Success");
        return StatusCode(response.StatusCode, response);
    }

    [HttpPost("finish")]
    public async Task<IActionResult> Finish(FinishPasskeyRegistrationRequest request)
    {
        // Check session
        var clientId = request.ClientId;
        var challengeJson = HttpContext.Session.GetString("fido2.registration.challenge");
        var sessionClientId = HttpContext.Session.GetString("fido2.registration.clientId");
        if (string.IsNullOrEmpty(challengeJson) || string.IsNullOrEmpty(sessionClientId) ||
            sessionClientId != clientId.ToString())
        {
            return StatusCode(400, ResponseModel.Error("Session invalid or expired, please refresh the page"));
        }

        // Check client
        var client = await clientRepository.GetByRegistrationEnabled(clientId);
        if (client == null)
        {
            return StatusCode(403, ResponseModel.Error("Access denied", 403));
        }

        var options = CredentialCreateOptions.FromJson(challengeJson);

        // Verify challenge
        RegisteredPublicKeyCredential credentialResult;
        try
        {
            credentialResult = await fido2.MakeNewCredentialAsync(new MakeNewCredentialParams
            {
                AttestationResponse = request.Credential,
                OriginalOptions = options,
                IsCredentialIdUniqueToUserCallback = async (args, ct) =>
                {
                    var exists = await userCredentialRepository.Query(c =>
                        c.CredentialId == args.CredentialId
                    ).AnyAsync(ct);
                    return !exists;
                }
            });
        }
        catch (Fido2VerificationException)
        {
            return StatusCode(401, ResponseModel.Error("Registration failed", 401));
        }

        var userId = new Guid(options.User.Id);
        var username = options.User.Name;

        // Check user existence
        if (await userRepository.IsExists(userId, username))
        {
            return StatusCode(400, ResponseModel.Error("User already exists"));
        }

        // Insert user
        var user = new UserEntity
        {
            Id = userId,
            Username = username,
            Credentials = new List<UserCredentialEntity>
            {
                new UserCredentialEntity
                {
                    UserId = userId,
                    CredentialId = credentialResult.Id,
                    PublicKey = credentialResult.PublicKey,
                    SignCount = 0
                }
            },
            ClientPermissions = new List<UserClientPermissionEntity>()
            {
                new UserClientPermissionEntity
                {
                    UserId = userId,
                    ClientId = clientId
                }
            }
        };
        await userRepository.Create(user);

        // Disable registration for admin client
        if (client.IsAdmin)
        {
            client.IsRegistrationEnabled = false;
            clientRepository.Update(client);
        }

        // Save all database changes
        await unitOfWork.SaveChangesAsync();

        // Clear session
        HttpContext.Session.Remove("fido2.registration.challenge");
        HttpContext.Session.Remove("fido2.registration.clientId");

        return StatusCode(200, ResponseModel.Success("Registration successful"));
    }
}