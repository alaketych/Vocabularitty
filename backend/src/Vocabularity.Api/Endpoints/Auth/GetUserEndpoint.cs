using FastEndpoints;
using Vocabularity.Service.Auth;
using Vocabularity.Service.Auth.Models;

namespace Vocabularity.Api.Endpoints.Auth;

public sealed class GetUserEndpoint(AuthService authService)
    : AuthenticatedEndpoint<EmptyRequest, UserResponse>
{
    public override void Configure()
    {
        if (Vocabularity.Api.Configuration.DemoMode.Enabled(Resolve<IWebHostEnvironment>(), Resolve<IConfiguration>())) AllowAnonymous();
        Get("/user/{id}");
        Description(builder => builder.WithTags("Users").Produces<UserResponse>(200));
    }

    public override async Task HandleAsync(EmptyRequest request, CancellationToken cancellationToken)
    {
        var response = await authService.GetUserAsync(
            CurrentUserId,
            Route<string>("id")!,
            cancellationToken,
            IsAdministrator);

        await Send.OkAsync(response, cancellationToken);
    }
}

