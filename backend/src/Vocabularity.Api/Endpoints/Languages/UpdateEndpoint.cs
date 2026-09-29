using FastEndpoints;
using Vocabularity.Service.Language;
using Vocabularity.Service.Language.Models;

namespace Vocabularity.Api.Endpoints.Languages;

public sealed class UpdateEndpoint(LanguageService languageService)
    : Endpoint<LanguageRequest, LanguageResponse>
{
    public override void Configure()
    {
        Put("/language/{id}");
        Description(builder => builder
            .WithTags("Language")
            .Produces<LanguageResponse>(200));
    }

    public override async Task HandleAsync(LanguageRequest request, CancellationToken cancellationToken)
    {
        var response = await languageService.UpdateAsync(Route<string>("id")!, request, cancellationToken);
        await Send.ResponseAsync(response, 200, cancellationToken);
    }
}


