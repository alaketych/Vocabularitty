using FastEndpoints;
using Vocabularity.Service.Dictionary;
using Vocabularity.Service.Dictionary.Models;

namespace Vocabularity.Api.Endpoints.Dictionaries;

public sealed class UpdateEndpoint(DictionaryService dictionaryService)
    : AuthenticatedEndpoint<DictionaryRequest, DictionaryResponse>
{
    public override void Configure()
    {
        Put("/dictionary/{id}");
        Description(builder => builder.Produces<DictionaryResponse>(200));
    }

    public override async Task HandleAsync(DictionaryRequest request, CancellationToken cancellationToken)
    {
        var response = await dictionaryService.UpdateAsync(CurrentUserId, Route<string>("id")!, request, cancellationToken);
        await Send.ResponseAsync(response, 200, cancellationToken);
    }
}


