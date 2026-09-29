using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Vocabularity.Infrastructure.Authentication;
using Vocabularity.Infrastructure.Database;
using Vocabularity.Service;
using Vocabularity.Service.Auth;
using Vocabularity.Service.Dictionary;
using Vocabularity.Service.User.Entities;

namespace Vocabularity.Api.Configuration;

public static class ApplicationServices
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration, IWebHostEnvironment environment)
    {
        services.AddProblemDetails();
        services.AddHttpContextAccessor();
        services.AddScoped<Vocabularity.Service.Activity.ICurrentActor, CurrentActor>();
        services.AddExceptionHandler<ApiExceptionHandler>();

        services.AddDbContext<VocabularityDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Missing database connection string."))
                .UseDemoData(DemoMode.Enabled(environment, configuration)));

        services.AddScoped<IVocabularityDbContext>(provider =>
            provider.GetRequiredService<VocabularityDbContext>());
        services.AddScoped<AuthService>();
        services.AddScoped<DictionaryService>();
        services.AddScoped<Vocabularity.Service.Language.LanguageService>();
        services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<ITokenService, JwtService>();

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.AddPolicy("auth", context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 20,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0
                    }));
        });

        return services;
    }
}

