using Application.Middlewares;
using Application.Models.Client;
using Application.Models.General.Request;
using Application.Models.General.Response;
using Application.Persistence;
using Application.Persistence.Client;
using Microsoft.AspNetCore.Mvc;

namespace Application.Controllers;

[Route("clients")]
[AdminAuthorize]
public class ClientsController(ClientRepository clientRepository, UnitOfWork unitOfWork) : Controller
{
    [HttpGet("create")]
    public IActionResult Create()
    {
        var model = new CreateClientViewModel()
        {
        };
        return View(model);
    }

    [HttpGet("update/{clientId:Guid}")]
    public async Task<IActionResult> Update([FromRoute] Guid clientId)
    {
        var client = await clientRepository.GetById(clientId);
        if (client == null)
        {
            return NotFound();
        }

        var model = new UpdateClientViewModel()
        {
            Client = client
        };
        return View(model);
    }

    [HttpGet("delete/{clientId:Guid}")]
    public async Task<IActionResult> Delete([FromRoute] Guid clientId)
    {
        var client = await clientRepository.GetById(clientId);
        if (client == null)
        {
            return NotFound();
        }
        clientRepository.Delete(client);
        await unitOfWork.SaveChangesAsync();
        return Redirect("/clients");
    }

    [HttpGet("")]
    public async Task<IActionResult> Index([FromQuery] SearchablePaginateRequest request)
    {
        var pq = clientRepository.CreatePaginationQuery(request.Search);
        var items = await clientRepository.GetPaginated(pq, request);
        var totalItemCount = await clientRepository.Count(pq);
        var pagination = new PaginateResponse<ClientEntity>
        {
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalItemCount = totalItemCount,
            Items = items
        };

        var model = new ListClientViewModel
        {
            Request = request,
            Pagination = pagination
        };
        return View(model);
    }
}