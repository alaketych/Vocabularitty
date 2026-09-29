using System.Net;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Vocabularity.Tests;

public sealed partial class ApiTests
{
    [Fact]
    public async Task Activities_record_successful_changes_and_are_private()
    {
        using var app = new ApiFactory();
        using var client = app.Start();
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/user/activities")).StatusCode);
        var token = await Register(client, "activity@example.com");
        Authorize(client, token);
        var language = await Body(await client.PostAsync("/language", Json(new { name = "English", original_name = "English" })));
        var created = await client.PostAsync("/dictionary", Json(new { dictionary_name = "Travel", language_id = "en" }));
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var dictionary = await Body(created);
        var path = $"/dictionary/{dictionary["id"]}";
        Assert.Equal(HttpStatusCode.OK, (await client.PutAsync(path, Json(new { dictionary_name = "Trips", language_id = "en" }))).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await client.DeleteAsync(path)).StatusCode);
        var before = JArray.Parse(await client.GetStringAsync("/user/activities"));
        var events = before.Where(item => (string?)item["entity_id"] == (string?)dictionary["id"]).ToList();
        Assert.Equal(3, events.Count);
        Assert.Contains(events, item => (string?)item["function"] == "create_dictionary");
        Assert.Contains(events, item => (string?)item["function"] == "update_dictionary");
        Assert.Contains(events, item => (string?)item["function"] == "delete_dictionary");
        Assert.All(events, item => Assert.NotNull(item["date"]));
        Assert.Equal(HttpStatusCode.NotFound, (await client.DeleteAsync(path)).StatusCode);
        Assert.Equal(before.Count, JArray.Parse(await client.GetStringAsync("/user/activities")).Count);
        Authorize(client, await Register(client, "other-activity@example.com"));
        var other = JArray.Parse(await client.GetStringAsync("/user/activities"));
        Assert.DoesNotContain(other, item => (string?)item["entity_id"] == (string?)dictionary["id"]);
    }
}
