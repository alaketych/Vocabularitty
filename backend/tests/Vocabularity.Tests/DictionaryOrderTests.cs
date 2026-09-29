using System.Net;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Vocabularity.Tests;

public sealed partial class ApiTests
{
    private static async Task<string> CreateDictionary(HttpClient client, string name)
    {
        var response = await client.PostAsync("/dictionary", Json(new
        {
            dictionary_name = name, language_id = "en"
        }));
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await Body(response)).Value<string>("id")!;
    }

    private static async Task<string[]> DictionaryOrder(HttpClient client)
    {
        var dictionaries = JArray.Parse(await client.GetStringAsync("/dictionaries"));
        return dictionaries.Select(dictionary => dictionary.Value<string>("id")!).ToArray();
    }

    [Fact]
    public async Task Ordering_persists_and_new_dictionaries_append_after_it()
    {
        using var app = new ApiFactory();
        using var client = app.Start();
        var token = await Register(client, "order@example.com");
        Authorize(client, token);
        var first = await CreateDictionary(client, "First");
        var second = await CreateDictionary(client, "Second");
        var third = await CreateDictionary(client, "Third");

        var reordered = await client.PutAsync("/dictionary/order", Json(new
        {
            dictionary_ids = new[] { third, first, second }
        }));
        Assert.Equal(HttpStatusCode.NoContent, reordered.StatusCode);

        using var freshClient = app.CreateClient();
        Authorize(freshClient, token);
        Assert.Equal(new[] { third, first, second }, await DictionaryOrder(freshClient));
        Assert.Equal(0, (await Body(await freshClient.GetAsync($"/dictionary/{third}"))).Value<int>("position"));
        var page = JArray.Parse(await freshClient.GetStringAsync("/dictionaries"));
        Assert.Equal(3, page.Count);
        Assert.Equal(third, page[0].Value<string>("id"));

        var appended = await CreateDictionary(client, "Fourth");
        Assert.Equal(new[] { third, first, second, appended }, await DictionaryOrder(client));
        await client.PutAsync($"/dictionary/{first}", Json(new { dictionary_name = "Renamed", language_id = "uk" }));
        Assert.Equal(new[] { third, first, second, appended }, await DictionaryOrder(client));
        await client.DeleteAsync($"/dictionary/{first}");
        Assert.Equal(new[] { third, second, appended }, await DictionaryOrder(client));
    }

    [Fact]
    public async Task Reorder_rejects_foreign_missing_and_duplicate_ids_without_changing_order()
    {
        using var app = new ApiFactory();
        using var client = app.Start();
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.PutAsync(
            "/dictionary/order", Json(new { dictionary_ids = Array.Empty<string>() }))).StatusCode);

        var owner = await Register(client, "owner-order@example.com");
        Authorize(client, owner);
        var first = await CreateDictionary(client, "First");
        var second = await CreateDictionary(client, "Second");
        var other = await Register(client, "other-order@example.com");
        Authorize(client, other);
        var foreign = await CreateDictionary(client, "Foreign");
        Authorize(client, owner);

        foreach (var invalidIds in new[]
        {
            new[] { first }, new[] { first, foreign },
            new[] { first, Guid.NewGuid().ToString() }, Array.Empty<string>()
        })
        {
            var response = await client.PutAsync("/dictionary/order", Json(new { dictionary_ids = invalidIds }));
            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
            Assert.Equal(new[] { first, second }, await DictionaryOrder(client));
        }

        Assert.Equal(HttpStatusCode.BadRequest, (await client.PutAsync(
            "/dictionary/order", Json(new { dictionary_ids = new[] { first, first } }))).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PutAsync("/dictionary/order", Json(new { }))).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PutAsync(
            "/dictionary/order", Json(new { dictionary_ids = (string[]?)null }))).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PutAsync(
            "/dictionary/order", Json(new { dictionary_ids = new[] { "" } }))).StatusCode);

        Authorize(client, other);
        Assert.Equal(new[] { foreign }, await DictionaryOrder(client));
    }

    [Fact]
    public async Task Empty_order_is_valid_for_a_user_without_dictionaries()
    {
        using var app = new ApiFactory();
        using var client = app.Start();
        Authorize(client, await Register(client, "empty-order@example.com"));
        Assert.Equal(HttpStatusCode.NoContent, (await client.PutAsync(
            "/dictionary/order", Json(new { dictionary_ids = Array.Empty<string>() }))).StatusCode);
    }
}

