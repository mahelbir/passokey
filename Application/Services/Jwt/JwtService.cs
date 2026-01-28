using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Application.Persistence.Client;
using Application.Persistence.User;
using Microsoft.IdentityModel.Tokens;

namespace Application.Services.Jwt;

public class JwtService(IConfiguration config)
{
    public string CreateToken(ClientEntity client, UserEntity user)
    {
        var jwtKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(client.SecretKey));
        var tokenData = new JwtSecurityToken(
            claims: new[]
            {
                new Claim("userId", user.Id.ToString()),
            },
            expires: DateTime.UtcNow.AddMinutes(config.GetValue<int>("Jwt:TokenLifetimeMinutes")),
            signingCredentials: new SigningCredentials(jwtKey, SecurityAlgorithms.HmacSha256)
        );
        return new JwtSecurityTokenHandler().WriteToken(tokenData);
    }

    public ClaimsPrincipal? ValidateToken(ClientEntity client, string token)
    {
        var jwtKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(client.SecretKey));
        var tokenHandler = new JwtSecurityTokenHandler();
        try
        {
            var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
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