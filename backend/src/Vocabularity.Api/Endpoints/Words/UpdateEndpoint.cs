using FastEndpoints;
using Vocabularity.Service.Dictionary;
using Vocabularity.Service.Dictionary.Models;

namespace Vocabularity.Api.Endpoints.Words;

public sealed class UpdateEndpoint(DictionaryService dictionaryService)
    : AuthenticatedEndpoint<WordRequest, WordResponse>
{
    public override void Configure()
    {
        Put("/dictionary/{id}/word/{wordId}");
        Description(builder => builder.Produces<WordResponse>(200));
    }

    public override async Task HandleAsync(WordRequest request, CancellationToken cancellationToken)
    {
        var response = await dictionaryService.UpdateWordAsync(CurrentUserId, Route<string>("id")!, Route<string>("wordId")!, request, cancellationToken);
        await Send.ResponseAsync(response, 200, cancellationToken);
    }
}


