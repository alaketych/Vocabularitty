using FastEndpoints;
using Vocabularity.Service.Dictionary;
using Vocabularity.Service.Dictionary.Models;

namespace Vocabularity.Api.Endpoints.Words;

public sealed class ListEndpoint(DictionaryService dictionaryService)
    : AuthenticatedListEndpoint< IReadOnlyList<WordResponse>>
{
    public override void Configure()
    {
        if (Vocabularity.Api.Configuration.DemoMode.Enabled(Resolve<IWebHostEnvironment>(), Resolve<IConfiguration>())) AllowAnonymous();
        Get("/dictionary/{id}/words");
        Description(builder => builder.Produces<IReadOnlyList<WordResponse>>(200));
    }

    public override async Task HandleAsync(CancellationToken cancellationToken)
    {
        var response = await dictionaryService.ListWordsAsync(CurrentUserId, Route<string>("id")!, cancellationToken, IsAdministrator);
        await Send.ResponseAsync(response, 200, cancellationToken);
    }
}



