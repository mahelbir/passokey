using Application.Models.PasskeyRegistration;
using Application.Persistence.Client;
using Microsoft.AspNetCore.Mvc;

namespace Application.Controllers;

public class PasskeyRegistrationController(ClientRepository clientRepository) : Controller
{
    [HttpGet("/auth/registration/{clientId:guid}")]
    public async Task<IActionResult> Index(Guid clientId, string? username = "")
    {
        // Check client
        ClientEntity? client = null;
        if (clientId != Guid.Empty)
        {
            client = await clientRepository.GetByRegistrationEnabled(clientId);
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
}