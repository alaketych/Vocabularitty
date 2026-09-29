using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Vocabularity.Service;
using Vocabularity.Service.Auth.Models;
using Vocabularity.Service.User.Entities;

namespace Vocabularity.Infrastructure.Authentication;

public sealed class JwtService(IOptions<JwtOptions> options) : ITokenService
{
    public AuthResponse Create(User user)
    {
        var settings = options.Value;
        var issuedAt = DateTime.UtcNow;
        var expiresAt = issuedAt.AddMinutes(settings.LifetimeMinutes);
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.Key));

        var token = new JwtSecurityToken(
            issuer: settings.Issuer,
            audience: settings.Audience,
            claims:
            [
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim("role", user.Role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            ],
            notBefore: issuedAt,
            expires: expiresAt,
            signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256));

        return new AuthResponse(
            new JwtSecurityTokenHandler().WriteToken(token),
            expiresAt,
            new UserResponse(user.Id, user.Email, user.Icon, user.Role));
    }
}

