using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Application.Persistence.Client;
using Microsoft.AspNetCore.WebUtilities;

namespace Application.Common;

public static class PasskeyHelper
{
    public static string GenerateClientSecretKey()
    {
        return Convert.ToHexString(RandomNumberGenerator.GetBytes(32)).ToLower();
    }

    public static string GetUsername(string? username)
    {
        if (username == null)
        {
            username = string.Empty;
        }

        username = Regex.Replace(username.Trim(), @"[^-a-zA-Z0-9._]", "");
        if (username.Length < 3 || username.Length > 255)
        {
            username = Convert.ToHexString(RandomNumberGenerator.GetBytes(4)).ToLower();
        }

        return username;
    }

    public static string? GetAuthenticatedRedirectUri(this ClientEntity client, string redirectUri, string token,
        string? state = null)
    {
        var validRedirectUri = client.GetResolvedRedirectUri(redirectUri);
        if (validRedirectUri == null)
        {
            return null;
        }

        redirectUri = validRedirectUri;

        var query = new Dictionary<string, string?>
        {
            ["token"] = token,
            ["clientId"] = client.Id.ToString()
        };

        if (!string.IsNullOrEmpty(state))
        {
            query["state"] = state;
        }

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