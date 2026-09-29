using FastEndpoints;
using Vocabularity.Service.Dictionary;
using Vocabularity.Service.Dictionary.Models;

namespace Vocabularity.Api.Endpoints.Words;

public sealed class UpdateEndpoint(DictionaryService dictionaryService)
    : AuthenticatedEndpoint<WordRequest, OperationResponse>
{
    public override void Configure()
    {
        Options(builder => builder.WithMetadata(new OperationResponseMetadata()));
        Put("/dictionary/{id}/word/{wordId}");
        Description(builder => builder.WithTags("Words").Produces<OperationResponse>(200));
    }

    public override async Task HandleAsync(WordRequest request, CancellationToken cancellationToken)
    {
        await dictionaryService.UpdateWordAsync(CurrentUserId, Route<string>("id")!, Route<string>("wordId")!, request, cancellationToken);
        await Send.ResponseAsync(new OperationResponse(true, "Word updated successfully."), 200, cancellationToken);
    }
}


