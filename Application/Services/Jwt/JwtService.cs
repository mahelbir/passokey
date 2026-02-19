using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Application.Common;
using Application.Persistence.Client;
using Application.Persistence.User;
using Microsoft.IdentityModel.Tokens;

namespace Application.Services.Jwt;

public class JwtService(IConfiguration config)
{
    public string CreateToken(ClientEntity client, UserEntity user, string? redirectUri = null)
    {
        var jwtKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(client.SecretKey));

        var claims = new List<Claim>
        {
            new("clientId", client.Id.ToString()),
            new("userId", user.Id.ToString())
        };

        // Extract audience from redirectUri
        if (!string.IsNullOrEmpty(redirectUri))
        {
            redirectUri = client.GetResolvedRedirectUri(redirectUri);
            if (redirectUri != null)
            {
                var uri = new Uri(redirectUri);
                claims.Add(new Claim("aud", uri.Host));
            }
        }

        var tokenData = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(config.GetValue<int>("Jwt:TokenLifetimeMinutes")),
            signingCredentials: new SigningCredentials(jwtKey, SecurityAlgorithms.HmacSha256)
        );
        return new JwtSecurityTokenHandler().WriteToken(tokenData);
    }

    public ClaimsPrincipal? ValidateToken(ClientEntity client, string token, string? audience = null)
    {
        if (!string.IsNullOrEmpty(audience))
        {
            var uri = UriHelper.ToUri(audience);
            if (uri != null) audience = uri.Host; // Extract audience from redirectUri
        }

        var jwtKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(client.SecretKey));
        var tokenHandler = new JwtSecurityTokenHandler();
        try
        {
            var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidAudience = audience,
                ValidateAudience = audience != null,
                ValidateLifetime = true,
                IssuerSigningKey = jwtKey,
                ValidateIssuerSigningKey = true,
                ClockSkew = TimeSpan.Zero
            }, out _);
            return principal;
        }
        catch
        {
            return null;
        }
    }
}