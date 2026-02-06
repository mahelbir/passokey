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

    public static string? GetAuthenticatedRedirectUri(ClientEntity client, string token, string? fallbackUri,
        string? state = "")
    {
        var redirectUri = GetValidRedirectUri(client, fallbackUri);
        if (redirectUri == null)
        {
            return null;
        }

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

    public static string? GetValidRedirectUri(ClientEntity client, string? fallbackUri = null)
    {
        var redirectUri = string.IsNullOrEmpty(client.RedirectUri)
            ? (string.IsNullOrEmpty(fallbackUri) ? null : fallbackUri)
            : client.RedirectUri;

        if (string.IsNullOrEmpty(redirectUri))
        {
            return null;
        }

        var uriResult = UriHelper.ToUri(redirectUri);
        if (uriResult == null || !uriResult.IsValidHttpUri())
        {
            return null;
        }

        return redirectUri;
    }
}