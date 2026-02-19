using Application.Common;
using Application.Middlewares;
using Application.Models.Client;
using Application.Models.General;
using Application.Models.General.Response;
using Application.Persistence;
using Application.Persistence.Client;
using Application.Services.Oidc;
using Microsoft.AspNetCore.Mvc;

namespace Application.Controllers.Api;

[Route("api/clients")]
[ApiController]
[AdminAuthorize]
public class ClientsApiController(
    ClientRepository clientRepository,
    UnitOfWork unitOfWork,
    OidcClientService oidcClientService
) : ControllerBase
{
    [HttpPost("create")]
    public async Task<IActionResult> Create(CreateClientRequest request)
    {
        var response = new ResponseModel<CreateResponse>();

        var client = new ClientEntity
        {
            Name = request.Name,
            SecretKey = PasskeyHelper.GenerateClientSecretKey(),
            IsRegistrationEnabled = true,
            RedirectUriList = request.RedirectUriList ?? []
        };
        await clientRepository.Create(client);
        await unitOfWork.SaveChangesAsync();
        await oidcClientService.CreateOrUpdateOidcApplication(client);

        response.Data = new CreateResponse
        {
            Id = client.Id
        };
        response.StatusCode = 201;
        response.Messages.Add("Client created successfully");

        return StatusCode(response.StatusCode, response);
    }

    [HttpPost("update")]
    public async Task<IActionResult> Update(UpdateClientRequest request)
    {
        var response = new ResponseModel();
        var client = await clientRepository.GetTrackedById(request.Id);
        if (client == null)
        {
            response.StatusCode = 404;
            response.Messages.Add("Client not found");
            return StatusCode(response.StatusCode, response);
        }

        if (!string.IsNullOrEmpty(request.Name)) client.Name = request.Name;

        if (request.RedirectUriList != null) client.RedirectUriList = request.RedirectUriList;

        client.IsRegistrationEnabled = request.IsRegistrationEnabled == "true";

        await unitOfWork.SaveChangesAsync();
        await oidcClientService.CreateOrUpdateOidcApplication(client);

        response.StatusCode = 200;
        response.Messages.Add("Client updated successfully");

        return StatusCode(response.StatusCode, response);
    }

    [HttpPost("change-secret-key/{id:Guid}")]
    public async Task<IActionResult> ChangeSecretKey(Guid id)
    {
        var response = new ResponseModel<ChangeSecretKeyResponse>();
        var client = await clientRepository.GetTrackedById(id);
        if (client == null)
        {
            response.StatusCode = 404;
            response.Messages.Add("Client not found");
            return StatusCode(response.StatusCode, response);
        }

        client.SecretKey = PasskeyHelper.GenerateClientSecretKey();

        await unitOfWork.SaveChangesAsync();
        response.Data = new ChangeSecretKeyResponse
        {
            SecretKey = client.SecretKey
        };
        response.StatusCode = 200;
        response.Messages.Add("Secret key updated successfully");

        return StatusCode(response.StatusCode, response);
    }
}