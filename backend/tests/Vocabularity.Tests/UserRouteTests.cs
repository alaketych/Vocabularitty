using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Vocabularity.Tests;

public sealed partial class ApiTests
{
    [Fact]
    public async Task User_route_returns_only_the_authenticated_users_profile()
    {
        using var app = new ApiFactory();
        using var client = app.Start();
        var registration = await client.PostAsync("/user/register", Json(new
        {
            email = "profile@example.com",
            password = "A-valid-password-123!"
        }));
        var account = await Body(registration);
        var userId = account["user"]!.Value<string>("id");
        var userUrl = $"/user/{userId}";

        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync(userUrl)).StatusCode);
        Authorize(client, account.Value<string>("access_token")!);

        var response = await client.GetAsync(userUrl);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var profile = await Body(response);
        Assert.Equal("profile@example.com", profile.Value<string>("email"));
        Assert.Null(profile["password_hash"]);
        Assert.Null(profile["normalized_email"]);

        Authorize(client, await Register(client, "other@example.com"));
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync(userUrl)).StatusCode);
    }

    [Fact]
    public async Task Swagger_ui_is_accessible_and_root_redirects_to_it()
    {
        using var app = new ApiFactory();
        using var initializedClient = app.Start();
        using var client = app.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var root = await client.GetAsync("/");
        Assert.Equal(HttpStatusCode.Redirect, root.StatusCode);
        Assert.Equal("/swagger", root.Headers.Location?.OriginalString);

        var swagger = await client.GetAsync("/swagger/index.html");
        Assert.Equal(HttpStatusCode.OK, swagger.StatusCode);
        Assert.Contains("Vocabularity API", await swagger.Content.ReadAsStringAsync());

        var document = await client.GetStringAsync("/swagger/v1/swagger.json");
        Assert.DoesNotContain("/api/", document);
        Assert.Contains("/dictionary/{id}/word/{wordId}", document);
    }
}

