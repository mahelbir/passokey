using Application.Common;
using Application.Middlewares;
using Application.Models.General.Request;
using Application.Models.General.Response;
using Application.Models.UserClientPermissions;
using Application.Persistence;
using Application.Persistence.Client;
using Application.Persistence.User;
using Application.Persistence.UserClientPermission;
using Microsoft.AspNetCore.Mvc;

namespace Application.Controllers;

[Route("user-client-permissions")]
[AdminAuthorize]
public class UserClientPermissionsController(
    UserClientPermissionRepository userClientPermissionRepository,
    ClientRepository clientRepository,
    UserRepository userRepository,
    UnitOfWork unitOfWork) : Controller
{
    [HttpGet("create/{clientId:guid}")]
    public async Task<IActionResult> Create(Guid clientId, [FromQuery] Guid userId)
    {
        var client = await clientRepository.GetById(clientId);
        if (client == null)
        {
            return NotFound();
        }

        var user = await userRepository.GetById(userId);
        if (user == null)
        {
            return NotFound();
        }

        var model = new CreateUserClientPermissionViewModel
        {
            Client = client,
            User = user
        };
        return View(model);
    }

    [HttpGet("delete/{permissionId:guid}")]
    public async Task<IActionResult> Delete([FromRoute] Guid permissionId)
    {
        var permission = await userClientPermissionRepository.GetById(permissionId);
        if (permission == null)
        {
            return NotFound();
        }

        var clientId = permission.ClientId;
        userClientPermissionRepository.Delete(permission);
        await unitOfWork.SaveChangesAsync();
        return Redirect($"/user-client-permissions/{clientId}");
    }

    [HttpGet("{clientId:guid}")]
    public async Task<IActionResult> Index([FromRoute] Guid clientId, [FromQuery] SearchablePaginateRequest request)
    {
        var client = await clientRepository.Get(c => c.Id == clientId);
        if (client == null)
        {
            return NotFound();
        }

        var pq = userClientPermissionRepository.CreatePaginationQuery(clientId, request.Search);
        var items = await userClientPermissionRepository.GetPaginated(pq, request);
        var totalItemCount = await userClientPermissionRepository.Count(pq);
        var pagination = new PaginateResponse<UserClientPermissionItem>
        {
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalItemCount = totalItemCount,
            Items = items
        };

        var model = new ListUserClientPermissionViewModel
        {
            Client = client,
            Request = request,
            Pagination = pagination,
            AdminUserId = HttpContext.Session.GetAdminUserId()
        };
        return View(model);
    }
}
