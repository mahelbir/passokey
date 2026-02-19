using Application.Persistence.Client;

namespace Application.Common;

public static class ClientAuthorizationHelper
{
    public static Guid? GetAuthorizedClientUserId(this ISession session, ClientEntity client)
    {
        var userIdString = session.GetString($"authorizedClient.{client.Id}.user");
        if (!string.IsNullOrEmpty(userIdString))
        {
            var expiresAtString = session.GetString($"authorizedClient.{client.Id}.expires");
            if (!string.IsNullOrEmpty(expiresAtString) && DateTime.TryParse(expiresAtString, out var expiresAt))
                if (DateTime.UtcNow < expiresAt)
                    return Guid.Parse(userIdString);
        }

        return null;
    }

    public static void ClearAuthorizedClientSession(this ISession session, Guid clientId)
    {
        session.Remove($"authorizedClient.{clientId}.user");
        session.Remove($"authorizedClient.{clientId}.expires");
    }

    public static void ClearAuthorizedClientSession(this ISession session, ClientEntity client)
    {
        session.ClearAuthorizedClientSession(client.Id);
    }
}