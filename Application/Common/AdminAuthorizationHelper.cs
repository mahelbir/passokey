namespace Application.Common;

public static class AdminAuthorizationHelper
{
    public static bool IsAdminAuthorized(this HttpContext httpContext)
    {
        var session = httpContext.Session;

        // Check if admin session exists
        var clientId = session.GetString("admin.client");
        var userId = session.GetString("admin.user");
        var expiresAtString = session.GetString("admin.expires");
        var storedUserAgent = session.GetString("admin.userAgent");

        if (string.IsNullOrEmpty(clientId) ||
            string.IsNullOrEmpty(userId) ||
            string.IsNullOrEmpty(expiresAtString) ||
            string.IsNullOrEmpty(storedUserAgent))
        {
            session.ClearAdminSession();
            return false;
        }

        // Check expiration
        if (!DateTime.TryParse(expiresAtString, out var expiresAt) || DateTime.UtcNow >= expiresAt)
        {
            session.ClearAdminSession();
            return false;
        }

        // Check user agent
        var currentUserAgent = httpContext.Request.Headers.UserAgent.ToString();
        if (storedUserAgent != currentUserAgent)
        {
            session.ClearAdminSession();
            return false;
        }

        return true;
    }

    public static void ClearAdminSession(this ISession session)
    {
        session.Remove("admin.client");
        session.Remove("admin.user");
        session.Remove("admin.expires");
        session.Remove("admin.userAgent");
    }
}