using FastEndpoints;
using Vocabularity.Service.Language;
using Vocabularity.Service.Language.Models;

namespace Vocabularity.Api.Endpoints.Languages;

public sealed class CreateEndpoint(LanguageService languageService)
    : Endpoint<LanguageRequest, LanguageResponse>
{
    public override void Configure()
    {
        Post("/language");
        Description(builder => builder
            .WithTags("Language")
            .Produces<LanguageResponse>(201));
    }

    public override async Task HandleAsync(LanguageRequest request, CancellationToken cancellationToken)
    {
        var response = await languageService.CreateAsync(request, cancellationToken);
        HttpContext.Response.Headers.Location = $"/language/{response.Id}";
        await Send.ResponseAsync(response, 201, cancellationToken);
    }
}


