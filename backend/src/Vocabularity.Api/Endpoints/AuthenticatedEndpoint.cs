using FastEndpoints;

namespace Vocabularity.Api.Endpoints;

public abstract class AuthenticatedEndpoint<TRequest, TResponse> : Endpoint<TRequest, TResponse>
    where TRequest : notnull
{
    // Ownership always comes from the validated JWT, never from request input.
    private bool IsAnonymousDemoRead => HttpContext.Request.Method == "GET" &&
        User.Identity?.IsAuthenticated != true &&
        Configuration.DemoMode.Enabled(Resolve<IWebHostEnvironment>(), Resolve<IConfiguration>());
    protected string CurrentUserId => User.FindFirst("sub")?.Value ?? "";
    protected bool IsAdministrator => IsAnonymousDemoRead || User.IsInRole(Vocabularity.Service.User.UserRoles.Administrator);
}


public abstract class AuthenticatedListEndpoint<TResponse> : EndpointWithoutRequest<TResponse>

{
    // Ownership always comes from the validated JWT, never from request input.
    private bool IsAnonymousDemoRead => HttpContext.Request.Method == "GET" &&
        User.Identity?.IsAuthenticated != true &&
        Configuration.DemoMode.Enabled(Resolve<IWebHostEnvironment>(), Resolve<IConfiguration>());
    protected string CurrentUserId => User.FindFirst("sub")?.Value ?? "";
    protected bool IsAdministrator => IsAnonymousDemoRead || User.IsInRole(Vocabularity.Service.User.UserRoles.Administrator);
}

