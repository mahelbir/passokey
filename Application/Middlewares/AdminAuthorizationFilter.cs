using Application.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Application.Middlewares;

public class AdminAuthorizationFilter : IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        if (!context.HttpContext.IsAdminAuthorized())
        {
            var currentPath = context.HttpContext.Request.Path + context.HttpContext.Request.QueryString;
            context.Result = new RedirectResult($"/admin/auth?state={Uri.EscapeDataString(currentPath)}");
        }
    }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AdminAuthorizeAttribute : TypeFilterAttribute
{
    public AdminAuthorizeAttribute() : base(typeof(AdminAuthorizationFilter))
    {
    }
}