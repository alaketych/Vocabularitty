using FastEndpoints;
using Vocabularity.Service.Dictionary;
using Vocabularity.Service.Dictionary.Models;

namespace Vocabularity.Api.Endpoints.Dictionaries;

public sealed class ListEndpoint(DictionaryService dictionaryService)
    : AuthenticatedListEndpoint< PageResponse<DictionaryResponse>>
{
    public override void Configure()
    {
        if (Vocabularity.Api.Configuration.DemoMode.Enabled(Resolve<IWebHostEnvironment>(), Resolve<IConfiguration>())) AllowAnonymous();
        Get("/dictionaries");
        Description(builder => builder.WithTags("Dictionaries").Produces<PageResponse<DictionaryResponse>>(200));
    }

    public override async Task HandleAsync(CancellationToken cancellationToken)
    {
        var paging = Pagination.Read(HttpContext.Request);
        var response = await dictionaryService.ListAsync(CurrentUserId, cancellationToken, IsAdministrator, paging.PageNumber, paging.PageSize);
        await Send.ResponseAsync(new PageResponse<DictionaryResponse>(paging.PageNumber, paging.PageSize, response), 200, cancellationToken);
    }
}



