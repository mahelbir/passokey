using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Fido2NetLib.Objects;

namespace Application.Common;

public static class PasskeyHelper
{
    public static UserVerificationRequirement UserVerification { get; set; } = UserVerificationRequirement.Required;

    public static string GenerateClientSecretKey()
    {
        return Convert.ToHexString(RandomNumberGenerator.GetBytes(32)).ToLower();
    }

    public static string GetUsername(string? username)
    {
        if (username == null) username = string.Empty;

        username = Regex.Replace(username.Trim(), @"[^-a-zA-Z0-9._]", "");
        if (username.Length < 3 || username.Length > 255)
            username = Convert.ToHexString(RandomNumberGenerator.GetBytes(4)).ToLower();

        return username;
    }
}