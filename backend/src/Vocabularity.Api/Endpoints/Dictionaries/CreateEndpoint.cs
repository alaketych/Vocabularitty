using FastEndpoints;
using Vocabularity.Service.Dictionary;
using Vocabularity.Service.Dictionary.Models;

namespace Vocabularity.Api.Endpoints.Dictionaries;

public sealed class CreateEndpoint(DictionaryService dictionaryService)
    : AuthenticatedEndpoint<DictionaryRequest, OperationResponse>
{
    public override void Configure()
    {
        Options(builder => builder.WithMetadata(new OperationResponseMetadata()));
        Post("/dictionary");
        Description(builder => builder.WithTags("Dictionaries").Produces<OperationResponse>(201));
    }

    public override async Task HandleAsync(DictionaryRequest request, CancellationToken cancellationToken)
    {
        var response = await dictionaryService.CreateAsync(CurrentUserId, request, cancellationToken);
        HttpContext.Response.Headers.Location = $"/dictionary/{response.Id}";
        await Send.ResponseAsync(new OperationResponse(true, "Dictionary created successfully."), 201, cancellationToken);
    }
}


