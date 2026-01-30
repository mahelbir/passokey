using Application.Middlewares;
using Application.Models.Client;
using Application.Models.General.Request;
using Application.Models.General.Response;
using Application.Persistence.Client;
using Microsoft.AspNetCore.Mvc;

namespace Application.Controllers;

[AdminAuthorize]
public class ClientsController(ClientRepository clientRepository) : Controller
{
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