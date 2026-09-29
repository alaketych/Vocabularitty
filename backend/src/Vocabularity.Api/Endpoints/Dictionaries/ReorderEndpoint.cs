using FastEndpoints;
using Vocabularity.Service.Dictionary;
using Vocabularity.Service.Dictionary.Models;

namespace Vocabularity.Api.Endpoints.Dictionaries;

public sealed class ReorderEndpoint(DictionaryService dictionaryService)
    : AuthenticatedEndpoint<ReorderDictionariesRequest, EmptyResponse>
{
    public override void Configure()
    {
        Put("/dictionary/order");
        Description(builder => builder.Produces(204).Produces(409));
    }

    public override async Task HandleAsync(
        ReorderDictionariesRequest request,
        CancellationToken cancellationToken)
    {
        await dictionaryService.ReorderAsync(CurrentUserId, request, cancellationToken);
        await Send.NoContentAsync(cancellationToken);
    }
}

