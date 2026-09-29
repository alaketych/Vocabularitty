using FastEndpoints;
using Vocabularity.Service.Auth;
using Vocabularity.Service.Auth.Models;

namespace Vocabularity.Api.Endpoints.Auth;

public sealed class RegisterEndpoint(AuthService authService) : Endpoint<RegisterRequest, AuthResponse>
{
    public override void Configure()
    {
        Post("/user/register");
        AllowAnonymous();
        Options(options => options.RequireRateLimiting("auth"));
        Description(builder => builder.WithTags("Authentication").Produces<AuthResponse>(201));
    }

    public override async Task HandleAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        var response = await authService.RegisterAsync(request, cancellationToken);
        await Send.ResponseAsync(response, 201, cancellationToken);
    }
}


