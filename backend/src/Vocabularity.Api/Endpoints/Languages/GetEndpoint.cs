using FastEndpoints;
using Vocabularity.Service.Language;
using Vocabularity.Service.Language.Models;

namespace Vocabularity.Api.Endpoints.Languages;

public sealed class GetEndpoint(LanguageService languageService)
    : Endpoint<EmptyRequest, LanguageResponse>
{
    public override void Configure()
    {
        if (Vocabularity.Api.Configuration.DemoMode.Enabled(Resolve<IWebHostEnvironment>(), Resolve<IConfiguration>())) AllowAnonymous();
        Get("/language/{id}");
        Description(builder => builder
            .WithTags("Languages")
            .Produces<LanguageResponse>(200));
    }

    public override async Task HandleAsync(EmptyRequest request, CancellationToken cancellationToken)
    {
        var response = await languageService.GetAsync(Route<string>("id")!, cancellationToken);
        await Send.ResponseAsync(response, 200, cancellationToken);
    }
}


