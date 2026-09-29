using FastEndpoints;
using Vocabularity.Service.Language;
using Vocabularity.Service.Language.Models;

namespace Vocabularity.Api.Endpoints.Languages;

public sealed class ListEndpoint(LanguageService languageService)
    : EndpointWithoutRequest<IReadOnlyList<LanguageResponse>>
{
    public override void Configure()
    {
        if (Vocabularity.Api.Configuration.DemoMode.Enabled(Resolve<IWebHostEnvironment>(), Resolve<IConfiguration>())) AllowAnonymous();
        Get("/languages");
        Description(builder => builder
            .WithTags("Language")
            .Produces<IReadOnlyList<LanguageResponse>>(200));
    }

    public override async Task HandleAsync(CancellationToken cancellationToken)
    {
        var response = await languageService.ListAsync(cancellationToken);
        await Send.ResponseAsync(response, 200, cancellationToken);
    }
}


