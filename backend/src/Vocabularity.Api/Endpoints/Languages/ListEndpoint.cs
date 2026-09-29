using FastEndpoints;
using Vocabularity.Service.Language;
using Vocabularity.Service.Language.Models;

namespace Vocabularity.Api.Endpoints.Languages;

public sealed class ListEndpoint(LanguageService languageService)
    : EndpointWithoutRequest<PageResponse<LanguageResponse>>
{
    public override void Configure()
    {
        if (Vocabularity.Api.Configuration.DemoMode.Enabled(Resolve<IWebHostEnvironment>(), Resolve<IConfiguration>())) AllowAnonymous();
        Get("/languages");
        Description(builder => builder
            .WithTags("Languages")
            .Produces<PageResponse<LanguageResponse>>(200));
    }

    public override async Task HandleAsync(CancellationToken cancellationToken)
    {
        var paging = Pagination.Read(HttpContext.Request);
        var response = await languageService.ListAsync(cancellationToken, paging.PageNumber, paging.PageSize);
        await Send.ResponseAsync(new PageResponse<LanguageResponse>(paging.PageNumber, paging.PageSize, response), 200, cancellationToken);
    }
}


