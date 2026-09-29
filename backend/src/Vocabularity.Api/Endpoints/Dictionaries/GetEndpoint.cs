using FastEndpoints;
using Vocabularity.Service.Dictionary;
using Vocabularity.Service.Dictionary.Models;

namespace Vocabularity.Api.Endpoints.Dictionaries;

public sealed class GetEndpoint(DictionaryService dictionaryService)
    : AuthenticatedEndpoint<EmptyRequest, DictionaryResponse>
{
    public override void Configure()
    {
        if (Vocabularity.Api.Configuration.DemoMode.Enabled(Resolve<IWebHostEnvironment>(), Resolve<IConfiguration>())) AllowAnonymous();
        Get("/dictionary/{id}");
        Description(builder => builder.Produces<DictionaryResponse>(200));
    }

    public override async Task HandleAsync(EmptyRequest request, CancellationToken cancellationToken)
    {
        var response = await dictionaryService.GetAsync(CurrentUserId, Route<string>("id")!, cancellationToken, IsAdministrator);
        await Send.ResponseAsync(response, 200, cancellationToken);
    }
}



