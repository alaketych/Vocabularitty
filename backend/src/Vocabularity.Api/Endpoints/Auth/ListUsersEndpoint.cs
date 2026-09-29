using FastEndpoints;
using Vocabularity.Service.Auth;
using Vocabularity.Service.Auth.Models;
using Vocabularity.Service.User;

namespace Vocabularity.Api.Endpoints.Auth;

public sealed class ListUsersEndpoint(AuthService authService)
    : AuthenticatedListEndpoint< IReadOnlyList<UserResponse>>
{
    public override void Configure()
    {
        if (Vocabularity.Api.Configuration.DemoMode.Enabled(Resolve<IWebHostEnvironment>(), Resolve<IConfiguration>())) AllowAnonymous();
        Get("/users");
        Roles(UserRoles.Administrator);
        Description(builder => builder.Produces<IReadOnlyList<UserResponse>>(200).Produces(403));
    }

    public override async Task HandleAsync(CancellationToken cancellationToken)
    {
        var response = await authService.ListUsersAsync(
            IsAdministrator, cancellationToken);
        await Send.OkAsync(response, cancellationToken);
    }
}
