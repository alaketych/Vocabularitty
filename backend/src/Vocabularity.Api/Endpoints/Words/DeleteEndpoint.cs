using FastEndpoints;
using Vocabularity.Service.Dictionary;
using Vocabularity.Service.Dictionary.Models;

namespace Vocabularity.Api.Endpoints.Words;

public sealed class DeleteEndpoint(DictionaryService dictionaryService)
    : AuthenticatedEndpoint<EmptyRequest, EmptyResponse>
{
    public override void Configure()
    {
        Delete("/dictionary/{id}/word/{wordId}");
        Description(builder => builder.Produces(204));
    }

    public override async Task HandleAsync(EmptyRequest request, CancellationToken cancellationToken)
    {
        await dictionaryService.DeleteWordAsync(CurrentUserId, Route<string>("id")!, Route<string>("wordId")!, cancellationToken);
        await Send.NoContentAsync(cancellationToken);
    }
}


