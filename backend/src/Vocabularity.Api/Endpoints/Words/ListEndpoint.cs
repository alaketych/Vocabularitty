using FastEndpoints;
using Vocabularity.Service.Dictionary;
using Vocabularity.Service.Dictionary.Models;

namespace Vocabularity.Api.Endpoints.Words;

public sealed class ListEndpoint(DictionaryService dictionaryService)
    : AuthenticatedListEndpoint< PageResponse<WordResponse>>
{
    public override void Configure()
    {
        if (Vocabularity.Api.Configuration.DemoMode.Enabled(Resolve<IWebHostEnvironment>(), Resolve<IConfiguration>())) AllowAnonymous();
        Get("/dictionary/{id}/words");
        Description(builder => builder.WithTags("Words").Produces<PageResponse<WordResponse>>(200));
    }

    public override async Task HandleAsync(CancellationToken cancellationToken)
    {
        var paging = Pagination.Read(HttpContext.Request);
        var response = await dictionaryService.ListWordsAsync(CurrentUserId, Route<string>("id")!, cancellationToken, IsAdministrator, paging.PageNumber, paging.PageSize);
        await Send.ResponseAsync(new PageResponse<WordResponse>(paging.PageNumber, paging.PageSize, response), 200, cancellationToken);
    }
}



