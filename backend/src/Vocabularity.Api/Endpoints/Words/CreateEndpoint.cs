using FastEndpoints;
using Vocabularity.Service.Dictionary;
using Vocabularity.Service.Dictionary.Models;

namespace Vocabularity.Api.Endpoints.Words;

public sealed class CreateEndpoint(DictionaryService dictionaryService)
    : AuthenticatedEndpoint<WordRequest, WordResponse>
{
    public override void Configure()
    {
        Post("/dictionary/{id}/word");
        Description(builder => builder.Produces<WordResponse>(201));
    }

    public override async Task HandleAsync(WordRequest request, CancellationToken cancellationToken)
    {
        var response = await dictionaryService.CreateWordAsync(CurrentUserId, Route<string>("id")!, request, cancellationToken);
        HttpContext.Response.Headers.Location = $"/dictionary/{response.DictionaryId}/word/{response.Id}";
        await Send.ResponseAsync(response, 201, cancellationToken);
    }
}


