using FastEndpoints;
using Vocabularity.Service.Dictionary;
using Vocabularity.Service.Dictionary.Models;

namespace Vocabularity.Api.Endpoints.Dictionaries;

public sealed class ReorderEndpoint(DictionaryService dictionaryService)
    : AuthenticatedEndpoint<ReorderDictionariesRequest, OperationResponse>
{
    public override void Configure()
    {
        Options(builder => builder.WithMetadata(new OperationResponseMetadata()));
        Put("/dictionary/order");
        Description(builder => builder.WithTags("Dictionaries").Produces<OperationResponse>(200).Produces<OperationResponse>(409));
    }

    public override async Task HandleAsync(
        ReorderDictionariesRequest request,
        CancellationToken cancellationToken)
    {
        await dictionaryService.ReorderAsync(CurrentUserId, request, cancellationToken);
        await Send.ResponseAsync(new OperationResponse(true, "Dictionary order updated successfully."), 200, cancellationToken);
    }
}

