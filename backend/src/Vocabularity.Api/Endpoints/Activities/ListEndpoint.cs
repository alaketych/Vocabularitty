using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Vocabularity.Infrastructure.Database;

namespace Vocabularity.Api.Endpoints.Activities;

public sealed record ActivityResponse(
    [property: JsonProperty("id")] string Id,
    [property: JsonProperty("user_id")] string UserId,
    [property: JsonProperty("function")] string Function,
    [property: JsonProperty("entity_id")] string EntityId,
    [property: JsonProperty("date")] DateTime Date);

public sealed class ListEndpoint(VocabularityDbContext database)
    : AuthenticatedListEndpoint< IReadOnlyList<ActivityResponse>>
{
    public override void Configure()
    {
        Get("/user/activities");
        Description(builder => builder.Produces<IReadOnlyList<ActivityResponse>>(200));
    }

    public override async Task HandleAsync(CancellationToken cancellationToken)
    {
        var activities = await database.UserActivities.AsNoTracking()
            .Where(activity => activity.UserId == CurrentUserId)
            .OrderByDescending(activity => activity.Date).ThenByDescending(activity => activity.Id)
            
            .Select(activity => new ActivityResponse(activity.Id, activity.UserId,
                activity.Function, activity.EntityId, activity.Date))
            .ToListAsync(cancellationToken);
        await Send.ResponseAsync(activities, 200, cancellationToken);
    }
}
