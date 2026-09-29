using FastEndpoints;
using Vocabularity.Service.Language;
using Vocabularity.Service.Language.Models;

namespace Vocabularity.Api.Endpoints.Languages;

public sealed class CreateEndpoint(LanguageService languageService)
    : Endpoint<LanguageRequest, OperationResponse>
{
    public override void Configure()
    {
        Post("/language");
        Description(builder => builder
            .WithTags("Languages")
            .Produces<OperationResponse>(201));
    }

    public override async Task HandleAsync(LanguageRequest request, CancellationToken cancellationToken)
    {
        var response = await languageService.CreateAsync(request, cancellationToken);
        HttpContext.Response.Headers.Location = $"/language/{response.Id}";
        await Send.ResponseAsync(new OperationResponse(true, "Language created successfully."), 201, cancellationToken);
    }
}


