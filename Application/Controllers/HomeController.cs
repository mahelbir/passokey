using Application.Common;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Application.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        if (HttpContext.IsAdminAuthorized())
        {
            return Redirect("/admin");
        }

        return Ok(HttpContext.Connection.RemoteIpAddress?.ToString());
    }

    [Route("auth/logout/{clientId:Guid}")]
    public IActionResult Logout(Guid clientId, string redirect = "")
    {
        HttpContext.Session.Remove($"authorizedClient.{clientId}.user");
        HttpContext.Session.Remove($"authorizedClient.{clientId}.expires");
        if (string.IsNullOrEmpty(redirect))
        {
            redirect = $"/auth/login/{clientId}";
        }

        // Open Redirect
        return Redirect(redirect);
    }

    [Route("Error/{statusCode}")]
    public IActionResult HttpStatusErrorCodeHandler(int statusCode)
    {
        var statusCodeFeature = HttpContext.Features.Get<IStatusCodeReExecuteFeature>();

        ViewData["StatusCode"] = statusCode;
        ViewData["OriginalPath"] = statusCodeFeature?.OriginalPath;
        ViewData["QueryString"] = statusCodeFeature?.OriginalQueryString;

        return View("Error");
    }

    [Route("Error")]
    public IActionResult Error()
    {
        var exceptionFeature = HttpContext.Features.Get<IExceptionHandlerFeature>();

        ViewData["StatusCode"] = 500;
        ViewData["ErrorMessage"] = exceptionFeature?.Error.Message;
        ViewData["StackTrace"] = exceptionFeature?.Error.StackTrace;
        ViewData["Path"] = exceptionFeature?.Path;

        return View("Error");
    }
}