using Application.Common;
using Application.Middlewares;
using Application.Models.General.Request;
using Application.Models.General.Response;
using Application.Models.User;
using Application.Persistence;
using Application.Persistence.User;
using Microsoft.AspNetCore.Mvc;

namespace Application.Controllers;

[Route("users")]
[AdminAuthorize]
public class UsersController(UserRepository userRepository, UnitOfWork unitOfWork) : Controller
{
    [HttpGet("update/{userId:guid}")]
    public async Task<IActionResult> Update([FromRoute] Guid userId)
    {
        var user = await userRepository.GetById(userId);
        if (user == null) return NotFound();

        var model = new UpdateUserViewModel
        {
            User = user
        };
        return View(model);
    }

    [HttpGet("delete/{userId:guid}")]
    public async Task<IActionResult> Delete([FromRoute] Guid userId)
    {
        var user = await userRepository.GetById(userId);
        if (user == null) return NotFound();
        userRepository.Delete(user);
        await unitOfWork.SaveChangesAsync();
        return Redirect("/users");
    }

    [HttpGet("")]
    public async Task<IActionResult> Index([FromQuery] SearchablePaginateRequest request,
        [FromQuery] Guid? permission = null)
    {
        var pq = userRepository.CreatePaginationQuery(request.Search);
        var items = await userRepository.GetPaginated(pq, request);
        var totalItemCount = await userRepository.Count(pq);
        var pagination = new PaginateResponse<UserEntity>
        {
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalItemCount = totalItemCount,
            Items = items
        };

        var model = new ListUserViewModel
        {
            Request = request,
            Pagination = pagination,
            AdminUserId = HttpContext.Session.GetAdminUserId(),
            PermissionClientId = permission
        };
        return View(model);
    }
}