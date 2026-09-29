using FastEndpoints;
using Vocabularity.Service.Dictionary;
using Vocabularity.Service.Dictionary.Models;

namespace Vocabularity.Api.Endpoints.Dictionaries;

public sealed class UpdateEndpoint(DictionaryService dictionaryService)
    : AuthenticatedEndpoint<DictionaryRequest, OperationResponse>
{
    public override void Configure()
    {
        Options(builder => builder.WithMetadata(new OperationResponseMetadata()));
        Put("/dictionary/{id}");
        Description(builder => builder.WithTags("Dictionaries").Produces<OperationResponse>(200));
    }

    public override async Task HandleAsync(DictionaryRequest request, CancellationToken cancellationToken)
    {
        await dictionaryService.UpdateAsync(CurrentUserId, Route<string>("id")!, request, cancellationToken);
        await Send.ResponseAsync(new OperationResponse(true, "Dictionary updated successfully."), 200, cancellationToken);
    }
}


