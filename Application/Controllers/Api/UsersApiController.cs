using Application.Middlewares;
using Application.Models.General;
using Application.Models.User;
using Application.Persistence;
using Application.Persistence.User;
using Microsoft.AspNetCore.Mvc;

namespace Application.Controllers.Api;

[Route("api/users")]
[ApiController]
[AdminAuthorize]
public class UsersApiController(UserRepository userRepository, UnitOfWork unitOfWork) : ControllerBase
{
    [HttpPost("update")]
    public async Task<IActionResult> Update(UpdateUserRequest request)
    {
        var response = new ResponseModel();
        var user = await userRepository.GetTrackedById(request.Id);
        if (user == null)
        {
            response.StatusCode = 404;
            response.Messages.Add("User not found");
            return StatusCode(response.StatusCode, response);
        }

        if (!string.IsNullOrEmpty(request.Username)) user.Username = request.Username;

        await unitOfWork.SaveChangesAsync();
        response.StatusCode = 200;
        response.Messages.Add("User updated successfully");

        return StatusCode(response.StatusCode, response);
    }
}