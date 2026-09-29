using FastEndpoints;
using Vocabularity.Service.Dictionary;
using Vocabularity.Service.Dictionary.Models;

namespace Vocabularity.Api.Endpoints.Dictionaries;

public sealed class CreateEndpoint(DictionaryService dictionaryService)
    : AuthenticatedEndpoint<DictionaryRequest, DictionaryResponse>
{
    public override void Configure()
    {
        Post("/dictionary");
        Description(builder => builder.Produces<DictionaryResponse>(201));
    }

    public override async Task HandleAsync(DictionaryRequest request, CancellationToken cancellationToken)
    {
        var response = await dictionaryService.CreateAsync(CurrentUserId, request, cancellationToken);
        HttpContext.Response.Headers.Location = $"/dictionary/{response.Id}";
        await Send.ResponseAsync(response, 201, cancellationToken);
    }
}


