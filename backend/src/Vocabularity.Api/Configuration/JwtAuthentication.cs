using System.Text;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Vocabularity.Infrastructure.Authentication;
using Vocabularity.Infrastructure.Database;

namespace Vocabularity.Api.Configuration;

public static class JwtAuthentication
{
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection("Jwt"))
            .Validate(options => Encoding.UTF8.GetByteCount(options.Key) >= 32,
                "Jwt:Key must contain at least 32 UTF-8 bytes.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.Issuer)
                && !string.IsNullOrWhiteSpace(options.Audience),
                "JWT issuer and audience are required.")
            .Validate(options => options.LifetimeMinutes is >= 1 and <= 1440,
                "JWT lifetime must be 1–1440 minutes.")
            .ValidateOnStart();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer();

        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<JwtOptions>>((options, configuredJwt) =>
            {
                var jwt = configuredJwt.Value;

                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwt.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwt.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
                    ValidateLifetime = true,
                    RequireExpirationTime = true,
                    RequireSignedTokens = true,
                    ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
                    ClockSkew = TimeSpan.FromSeconds(30),
                    NameClaimType = "sub",
                    RoleClaimType = "role"
                };

                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = ValidateActiveUserAsync
                };
            });

        services.AddAuthorization();
        return services;
    }

    private static async Task ValidateActiveUserAsync(TokenValidatedContext context)
    {
        var userId = context.Principal?.FindFirst("sub")?.Value;
        var database = context.HttpContext.RequestServices
            .GetRequiredService<VocabularityDbContext>();

        var user = await database.Users.AsNoTracking().SingleOrDefaultAsync(
            user => user.Id == userId && user.IsActive,
            context.HttpContext.RequestAborted);

        if (user is null)
        {
            context.Fail("User is unavailable.");
            return;
        }

        // Use the current database role, so demotion takes effect even for existing tokens.
        var identity = (ClaimsIdentity)context.Principal!.Identity!;
        foreach (var claim in identity.FindAll("role").ToList())
        {
            identity.RemoveClaim(claim);
        }
        identity.AddClaim(new Claim("role", user.Role));
    }
}

