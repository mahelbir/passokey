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

[Route("auth/registration")]
public class PasskeyRegistrationController(
    ClientRepository clientRepository,
    UserRepository userRepository,
    UserCredentialRepository userCredentialRepository,
    UnitOfWork unitOfWork,
    IFido2 fido2) : Controller
{
    [HttpGet("{clientId:Guid}")]
    public async Task<IActionResult> Index(Guid clientId, string? username = "")
    {
        ClientEntity? client = null;
        if (clientId != Guid.Empty)
        {
            client = await clientRepository.Get(c =>
                c.Id == clientId &&
                c.IsRegistrationEnabled == true
            ).AsNoTracking().FirstOrDefaultAsync();
        }

        if (client == null)
        {
            return NotFound();
        }

        var model = new PasskeyRegistrationViewModel
        {
            ClientId = clientId,
            Username = username
        };

        return View(model);
    }

    [HttpPost("start/{clientId:Guid}")]
    public async Task<IActionResult> Start(Guid clientId,
        [FromBody] StartPasskeyRegistrationRequest request)
    {
        ClientEntity? client = null;
        if (clientId != Guid.Empty)
        {
            client = await clientRepository.Get(c =>
                c.Id == clientId &&
                c.IsRegistrationEnabled == true
            ).AsNoTracking().FirstOrDefaultAsync();
        }

        if (client == null)
        {
            return StatusCode(403, ResponseModel.Error("Access denied", 403));
        }

        var response = new ResponseModel<StartPasskeyRegistrationResponse>();
        var username = PasskeyHelper.GetUsername(request.Username);
        var userId = Guid.NewGuid().ToByteArray();

        var user = new Fido2User
        {
            Id = userId,
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
        return Json(response);
    }

    [HttpPost("finish/{clientId:Guid}")]
    public async Task<IActionResult> Finish(Guid clientId,
        [FromBody] FinishPasskeyRegistrationRequest request)
    {
        // Check session
        var challengeJson = HttpContext.Session.GetString("fido2.registration.challenge");
        var sessionClientId = HttpContext.Session.GetString("fido2.registration.clientId");

        if (string.IsNullOrEmpty(challengeJson) || string.IsNullOrEmpty(sessionClientId) ||
            sessionClientId != clientId.ToString())
        {
            return Json(ResponseModel.Error("Invalid session"));
        }

        // Check client
        var client = await clientRepository.Get(c =>
            c.Id == clientId &&
            c.IsRegistrationEnabled == true
        ).AsNoTracking().FirstOrDefaultAsync();

        if (client == null)
        {
            return Json(ResponseModel.Error("Access denied", 403));
        }

        var options = CredentialCreateOptions.FromJson(challengeJson);

        // Check credential
        RegisteredPublicKeyCredential credentialResult;
        try
        {
            credentialResult = await fido2.MakeNewCredentialAsync(new MakeNewCredentialParams
            {
                AttestationResponse = request.Credential,
                OriginalOptions = options,
                IsCredentialIdUniqueToUserCallback = async (args, ct) =>
                {
                    var exists = await userCredentialRepository.Get(c =>
                        c.CredentialId == args.CredentialId
                    ).AsNoTracking().AnyAsync(ct);
                    return !exists;
                }
            });
        }
        catch (Fido2VerificationException)
        {
            return Json(ResponseModel.Error("Registration failed"));
        }

        // Check user existence
        var isExists = await userRepository.IsExists(u =>
            u.Id == new Guid(options.User.Id) ||
            u.Username == options.User.Name
        );
        if (isExists)
        {
            return Json(ResponseModel.Error("User already exists"));
        }

        // Insert user
        var userId = new Guid(options.User.Id);
        var user = new UserEntity
        {
            Id = userId,
            Username = options.User.Name,
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

        return Json(ResponseModel.Success("Registration successful"));
    }
}