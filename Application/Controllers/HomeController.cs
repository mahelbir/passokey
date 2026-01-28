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