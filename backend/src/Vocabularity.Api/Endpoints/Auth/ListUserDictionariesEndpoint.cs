using FastEndpoints;
using Vocabularity.Service.Auth;
using Vocabularity.Service.Dictionary;
using Vocabularity.Service.Dictionary.Models;

namespace Vocabularity.Api.Endpoints.Auth;

public sealed class ListUserDictionariesEndpoint(
    AuthService authService,
    DictionaryService dictionaryService)
    : AuthenticatedListEndpoint< IReadOnlyList<DictionaryResponse>>
{
    public override void Configure()
    {
        if (Vocabularity.Api.Configuration.DemoMode.Enabled(Resolve<IWebHostEnvironment>(), Resolve<IConfiguration>())) AllowAnonymous();
        Get("/user/{id}/dictionaries");
        Description(builder => builder.Produces<IReadOnlyList<DictionaryResponse>>(200));
    }

    public override async Task HandleAsync(CancellationToken cancellationToken)
    {
        var requestedUserId = Route<string>("id")!;
        await authService.GetUserAsync(CurrentUserId, requestedUserId, cancellationToken, IsAdministrator);
        var response = await dictionaryService.ListAsync(
            requestedUserId, cancellationToken);
        await Send.OkAsync(response, cancellationToken);
    }
}
