using FastEndpoints;
using Vocabularity.Service.Dictionary;
using Vocabularity.Service.Dictionary.Models;

namespace Vocabularity.Api.Endpoints.Words;

public sealed class GetEndpoint(DictionaryService dictionaryService)
    : AuthenticatedEndpoint<EmptyRequest, WordResponse>
{
    public override void Configure()
    {
        if (Vocabularity.Api.Configuration.DemoMode.Enabled(Resolve<IWebHostEnvironment>(), Resolve<IConfiguration>())) AllowAnonymous();
        Get("/dictionary/{id}/word/{wordId}");
        Description(builder => builder.Produces<WordResponse>(200));
    }

    public override async Task HandleAsync(EmptyRequest request, CancellationToken cancellationToken)
    {
        var response = await dictionaryService.GetWordAsync(CurrentUserId, Route<string>("id")!, Route<string>("wordId")!, cancellationToken, IsAdministrator);
        await Send.ResponseAsync(response, 200, cancellationToken);
    }
}



