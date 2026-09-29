using FastEndpoints;
using Vocabularity.Service.Dictionary;
using Vocabularity.Service.Dictionary.Models;

namespace Vocabularity.Api.Endpoints.Dictionaries;

public sealed class DeleteEndpoint(DictionaryService dictionaryService)
    : AuthenticatedEndpoint<EmptyRequest, EmptyResponse>
{
    public override void Configure()
    {
        Delete("/dictionary/{id}");
        Description(builder => builder.WithTags("Dictionaries").Produces(204));
    }

    public override async Task HandleAsync(EmptyRequest request, CancellationToken cancellationToken)
    {
        await dictionaryService.DeleteAsync(CurrentUserId, Route<string>("id")!, cancellationToken);
        await Send.NoContentAsync(cancellationToken);
    }
}


