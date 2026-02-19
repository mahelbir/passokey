using Application.Persistence.Client;
using Microsoft.AspNetCore.WebUtilities;

namespace Application.Common;

public static class UriHelper
{
    public static bool IsValidHttpUri(this Uri uri)
    {
        return uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps;
    }

    public static Uri? ToUri(string uriString)
    {
        return Uri.TryCreate(uriString, UriKind.Absolute, out var uri) ? uri : null;
    }

    public static string? GetLocalReturnPath(string? returnPath)
    {
        return !string.IsNullOrEmpty(returnPath) && returnPath.StartsWith('/') ? returnPath : null;
    }

    public static string? GetAuthenticatedRedirectUri(this ClientEntity client, string? redirectUri, string token,
        string? state = null)
    {
        var validRedirectUri = client.GetResolvedRedirectUri(redirectUri);
        if (validRedirectUri == null) return null;

        redirectUri = validRedirectUri;

        var query = new Dictionary<string, string?>
        {
            ["token"] = token,
            ["clientId"] = client.Id.ToString()
        };

        if (!string.IsNullOrEmpty(state)) query["state"] = state;

        var url = QueryHelpers.AddQueryString(redirectUri, query);
        return url;
    }

    public static string? GetResolvedRedirectUri(this ClientEntity client, string? redirectUri)
    {
        if (string.IsNullOrWhiteSpace(redirectUri) ||
            !Uri.TryCreate(redirectUri, UriKind.Absolute, out var requestedUri))
            return null;

        return client.RedirectUriList
            .Where(u => Uri.TryCreate(u, UriKind.Absolute, out _))
            .Select(u => new Uri(u))
            .FirstOrDefault(u => u.Scheme == requestedUri.Scheme
                                 && u.Host == requestedUri.Host
                                 && u.Port == requestedUri.Port
                                 && u.AbsolutePath.TrimEnd('/') == requestedUri.AbsolutePath.TrimEnd('/'))
            ?.ToString();
    }
}