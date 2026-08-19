using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using UnderworldAPI.Shared.Domain.Auth;

namespace UnderworldAPI.Sales.Infrastructure.Auth;

/// <summary>
/// Implementación de ITokenGenerator usando JWT.
/// Lee la secret key de Key Vault en producción o appsettings en desarrollo.
/// </summary>
internal sealed class JwtTokenGenerator(IConfiguration configuration) : ITokenGenerator
{
    public string GenerateToken(string userId, string email, string role)
    {
        // Lee la key de Key Vault (producción) o appsettings (desarrollo)
        var secretKey = configuration["JwtSecretKey"]
            ?? configuration["Jwt:SecretKey"]
            ?? throw new InvalidOperationException("JWT Secret Key not configured.");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(ClaimTypes.Role, role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat,
                DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                ClaimValueTypes.Integer64)
        };

        var token = new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"] ?? "UnderworldAPI",
            audience: configuration["Jwt:Audience"] ?? "UnderworldAPI.Client",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}