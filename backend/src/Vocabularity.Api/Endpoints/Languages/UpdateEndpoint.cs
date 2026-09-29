using FastEndpoints;
using Vocabularity.Service.Language;
using Vocabularity.Service.Language.Models;

namespace Vocabularity.Api.Endpoints.Languages;

public sealed class UpdateEndpoint(LanguageService languageService)
    : Endpoint<LanguageRequest, OperationResponse>
{
    public override void Configure()
    {
        Put("/language/{id}");
        Description(builder => builder
            .WithTags("Languages")
            .Produces<OperationResponse>(200));
    }

    public override async Task HandleAsync(LanguageRequest request, CancellationToken cancellationToken)
    {
        await languageService.UpdateAsync(Route<string>("id")!, request, cancellationToken);
        await Send.ResponseAsync(new OperationResponse(true, "Language updated successfully."), 200, cancellationToken);
    }
}


