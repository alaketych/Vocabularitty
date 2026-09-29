using FastEndpoints;
using Vocabularity.Service.Language;
using Vocabularity.Service.Language.Models;

namespace Vocabularity.Api.Endpoints.Languages;

public sealed class DeleteEndpoint(LanguageService languageService)
    : Endpoint<EmptyRequest, EmptyResponse>
{
    public override void Configure()
    {
        Delete("/language/{id}");
        Description(builder => builder
            .WithTags("Language")
            .Produces(204));
    }

    public override async Task HandleAsync(EmptyRequest request, CancellationToken cancellationToken)
    {
        await languageService.DeleteAsync(Route<string>("id")!, cancellationToken);
        await Send.NoContentAsync(cancellationToken);
    }
}


