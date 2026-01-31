using Application.Middlewares;
using Application.Models.General;
using Application.Models.General.Response;
using Application.Models.UserClientPermissions;
using Application.Persistence;
using Application.Persistence.UserClientPermission;
using Microsoft.AspNetCore.Mvc;

namespace Application.Controllers;

[Route("api/user-client-permissions")]
[ApiController]
[AdminAuthorize]
public class UserClientPermissionsApiController(UserClientPermissionRepository userClientPermissionRepository, UnitOfWork unitOfWork) : ControllerBase
{
    [HttpPost("create")]
    public async Task<IActionResult> Create(CreateUserClientPermissionRequest request)
    {
        var response = new ResponseModel<CreateResponse>();

        var permission = new UserClientPermissionEntity
        {
            UserId = request.UserId,
            ClientId = request.ClientId
        };

        if (await userClientPermissionRepository.IsExists(permission))
        {
            response.StatusCode = 400;
            response.Messages.Add("Permission already exists");
            return StatusCode(response.StatusCode, response);
        }

        await userClientPermissionRepository.Create(permission);
        await unitOfWork.SaveChangesAsync();

        response.Data = new CreateResponse
        {
            Id = permission.Id
        };
        response.StatusCode = 201;
        response.Messages.Add("Permission created successfully");

        return StatusCode(response.StatusCode, response);
    }
}
