using FastEndpoints;
using Vocabularity.Service.Dictionary;
using Vocabularity.Service.Dictionary.Models;

namespace Vocabularity.Api.Endpoints.Dictionaries;

public sealed class ListEndpoint(DictionaryService dictionaryService)
    : AuthenticatedListEndpoint< IReadOnlyList<DictionaryResponse>>
{
    public override void Configure()
    {
        if (Vocabularity.Api.Configuration.DemoMode.Enabled(Resolve<IWebHostEnvironment>(), Resolve<IConfiguration>())) AllowAnonymous();
        Get("/dictionaries");
        Description(builder => builder.Produces<IReadOnlyList<DictionaryResponse>>(200));
    }

    public override async Task HandleAsync(CancellationToken cancellationToken)
    {
        var response = await dictionaryService.ListAsync(CurrentUserId, cancellationToken, IsAdministrator);
        await Send.ResponseAsync(response, 200, cancellationToken);
    }
}



