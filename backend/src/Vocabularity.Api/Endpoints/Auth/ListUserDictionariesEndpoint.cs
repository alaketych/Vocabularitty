using FastEndpoints;
using Vocabularity.Service.Auth;
using Vocabularity.Service.Dictionary;
using Vocabularity.Service.Dictionary.Models;

namespace Vocabularity.Api.Endpoints.Auth;

public sealed class ListUserDictionariesEndpoint(
    AuthService authService,
    DictionaryService dictionaryService)
    : AuthenticatedListEndpoint< PageResponse<DictionaryResponse>>
{
    public override void Configure()
    {
        if (Vocabularity.Api.Configuration.DemoMode.Enabled(Resolve<IWebHostEnvironment>(), Resolve<IConfiguration>())) AllowAnonymous();
        Get("/user/{id}/dictionaries");
        Description(builder => builder.WithTags("Users").Produces<PageResponse<DictionaryResponse>>(200));
    }

    public override async Task HandleAsync(CancellationToken cancellationToken)
    {
        var paging = Pagination.Read(HttpContext.Request);
        var requestedUserId = Route<string>("id")!;
        await authService.GetUserAsync(CurrentUserId, requestedUserId, cancellationToken, IsAdministrator);
        var response = await dictionaryService.ListAsync(
            requestedUserId, cancellationToken, pageNumber: paging.PageNumber, pageSize: paging.PageSize);
        await Send.OkAsync(new PageResponse<DictionaryResponse>(paging.PageNumber, paging.PageSize, response), cancellationToken);
    }
}
