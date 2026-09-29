using FastEndpoints;
using Vocabularity.Service.Auth;
using Vocabularity.Service.Auth.Models;

namespace Vocabularity.Api.Endpoints.Auth;

public sealed class LoginEndpoint(AuthService authService) : Endpoint<LoginRequest, AuthResponse>
{
    public override void Configure()
    {
        Post("/user/login");
        AllowAnonymous();
        Options(options => options.RequireRateLimiting("auth"));
        Description(builder => builder.WithTags("Authentication").Produces<AuthResponse>(200));
    }

    public override async Task HandleAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        if (Configuration.DemoMode.Enabled(Resolve<IWebHostEnvironment>(), Resolve<IConfiguration>()) &&
            request.Username == "admin" && string.IsNullOrEmpty(request.Email))
            request.Email = Vocabularity.Infrastructure.Database.DemoData.AdminEmail;
        var response = await authService.LoginAsync(request, cancellationToken);
        await Send.ResponseAsync(response, 200, cancellationToken);
    }
}


