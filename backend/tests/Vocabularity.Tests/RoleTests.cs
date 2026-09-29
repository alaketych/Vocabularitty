using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Vocabularity.Infrastructure.Database;
using Vocabularity.Service.User;
using Xunit;

namespace Vocabularity.Tests;

public sealed partial class ApiTests
{
    private static async Task SetRole(ApiFactory app, string email, string role)
    {
        using var scope = app.Services.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<VocabularityDbContext>();
        var user = await database.Users.SingleAsync(user => user.Email == email);
        user.Role = role;
        await database.SaveChangesAsync();
    }

    [Fact]
    public async Task Administrator_can_read_all_users_and_dictionaries_but_not_change_another_owners_data()
    {
        using var app = new ApiFactory();
        using var client = app.Start();
        var ownerToken = await Register(client, "role-owner@example.com");
        Authorize(client, ownerToken);
        var dictionaryId = await CreateDictionary(client, "Private");
        var dictionary = await Body(await client.GetAsync($"/dictionary/{dictionaryId}"));
        var ownerId = dictionary.Value<string>("user_id");
        var word = await Body(await client.PostAsync($"/dictionary/{dictionaryId}/word",
            Json(new { original_word = "cat", translated_word = "кіт" })));
        var wordPath = $"/dictionary/{dictionaryId}/word/{word["id"]}";

        var adminToken = await Register(client, "role-admin@example.com");
        await SetRole(app, "role-admin@example.com", UserRoles.Administrator);
        Authorize(client, adminToken);

        var usersResponse = await client.GetAsync("/users");
        Assert.Equal(HttpStatusCode.OK, usersResponse.StatusCode);
        Assert.Equal(2, JArray.Parse(await usersResponse.Content.ReadAsStringAsync()).Count);
        Assert.DoesNotContain("password_hash", await usersResponse.Content.ReadAsStringAsync());
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync($"/user/{ownerId}")).StatusCode);
        Assert.Contains(dictionaryId, await client.GetStringAsync("/dictionaries"));
        Assert.Contains(dictionaryId, await client.GetStringAsync($"/user/{ownerId}/dictionaries"));
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync($"/dictionary/{dictionaryId}")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync($"/dictionary/{dictionaryId}/words")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync(wordPath)).StatusCode);

        Assert.Equal(HttpStatusCode.NotFound, (await client.DeleteAsync($"/dictionary/{dictionaryId}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.PutAsync($"/dictionary/{dictionaryId}",
            Json(new { dictionary_name = "Changed", language_id = "en" }))).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.PostAsync($"/dictionary/{dictionaryId}/word",
            Json(new { original_word = "dog", translated_word = "пес" }))).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.PutAsync(wordPath,
            Json(new { original_word = "dog", translated_word = "пес" }))).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.DeleteAsync(wordPath)).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await client.PutAsync("/dictionary/order",
            Json(new { dictionary_ids = new[] { dictionaryId } }))).StatusCode);

        // Demotion must apply even though the same JWT remains valid.
        await SetRole(app, "role-admin@example.com", UserRoles.User);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync("/users")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"/user/{ownerId}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"/user/{ownerId}/dictionaries")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"/dictionary/{dictionaryId}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync(wordPath)).StatusCode);
        Assert.Equal("[]", await client.GetStringAsync("/dictionaries"));

        Authorize(client, ownerToken);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync($"/user/{ownerId}/dictionaries")).StatusCode);
    }

    [Fact]
    public async Task Registration_cannot_select_a_role_and_login_reports_the_saved_role()
    {
        using var app = new ApiFactory();
        using var client = app.Start();
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/users")).StatusCode);
        var forged = await client.PostAsync("/user/register", Json(new
        {
            email = "forged@example.com", password = "Test-password-12345!", role = "Administrator"
        }));
        Assert.Equal(HttpStatusCode.BadRequest, forged.StatusCode);

        var registered = await Body(await client.PostAsync("/user/register", Json(new
        {
            email = "default-role@example.com", password = "Test-password-12345!"
        })));
        Assert.Equal("User", registered["user"]!.Value<string>("role"));
        Authorize(client, registered.Value<string>("access_token")!);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync("/users")).StatusCode);

        await SetRole(app, "default-role@example.com", UserRoles.Administrator);
        var login = await Body(await client.PostAsync("/user/login", Json(new
        {
            email = "default-role@example.com", password = "Test-password-12345!"
        })));
        Assert.Equal("Administrator", login["user"]!.Value<string>("role"));
        Authorize(client, login.Value<string>("access_token")!);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/users")).StatusCode);
    }
}
