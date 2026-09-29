using System.Net;
using Xunit;

namespace Vocabularity.Tests;

public sealed partial class ApiTests
{
    [Fact]
    public async Task Languages_are_shared_and_support_full_crud()
    {
        using var app = new ApiFactory();
        using var client = app.Start();
        Authorize(client, await Register(client, "language-owner@example.com"));

        var created = await client.PostAsync("/language", Json(new
        {
            name = " Ukrainian ",
            original_name = " Українська ",
            icon = "🇺🇦"
        }));
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var language = await Body(created);
        var path = $"/language/{language["id"]}";
        Assert.Equal(path, created.Headers.Location?.OriginalString);
        Assert.Equal("Ukrainian", language.Value<string>("name"));
        Assert.Equal("Українська", language.Value<string>("original_name"));
        Assert.Equal("🇺🇦", language.Value<string>("icon"));

        Authorize(client, await Register(client, "language-reader@example.com"));
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync(path)).StatusCode);
        Assert.Contains("Ukrainian", await client.GetStringAsync("/languages"));

        var updated = await client.PutAsync(path, Json(new
        {
            name = "Ukrainian",
            original_name = "Українська"
        }));
        Assert.Equal(HttpStatusCode.OK, updated.StatusCode);
        Assert.Null((await Body(updated)).Value<string>("icon"));

        Assert.Equal(HttpStatusCode.NoContent, (await client.DeleteAsync(path)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync(path)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.DeleteAsync(path)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.PutAsync(path, Json(new
        {
            name = "Ukrainian", original_name = "Українська"
        }))).StatusCode);
    }

    [Fact]
    public async Task Language_requests_require_authentication_and_valid_fields()
    {
        using var app = new ApiFactory();
        using var client = app.Start();
        var valid = new { name = "German", original_name = "Deutsch" };

        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/languages")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/language/unknown")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.PostAsync("/language", Json(valid))).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.PutAsync("/language/unknown", Json(valid))).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.DeleteAsync("/language/unknown")).StatusCode);

        Authorize(client, await Register(client, "language-validator@example.com"));
        foreach (var invalid in new object[]
        {
            new { name = " ", original_name = "Deutsch" },
            new { name = "German", original_name = "" },
            new { name = new string('x', 101), original_name = "Deutsch" },
            new { name = "German", original_name = new string('x', 101) },
            new { name = "German", original_name = "Deutsch", icon = new string('x', 2049) },
            new { name = "German", original_name = "Deutsch", user_id = "forged" }
        })
        {
            Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsync("/language", Json(invalid))).StatusCode);
        }

        var created = await client.PostAsync("/language", Json(valid));
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var language = await Body(created);
        Assert.Null(language.Value<string>("icon"));

        var invalidUpdate = await client.PutAsync($"/language/{language["id"]}", Json(new
        {
            name = "German", original_name = " "
        }));
        Assert.Equal(HttpStatusCode.BadRequest, invalidUpdate.StatusCode);
    }
}

