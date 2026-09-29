using System.Net;
using System.Text;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Vocabularity.Tests;

public sealed partial class ApiTests
{
    [Theory]
    [InlineData("bad-email", "A-valid-password-123!", null, "email")]
    [InlineData("valid@example.com", "short", null, "password")]
    [InlineData("valid@example.com", "A-valid-password-123!", "not-a-url", "icon")]
    public async Task Registration_uses_fluent_validation(
        string email,
        string password,
        string? icon,
        string expectedField)
    {
        using var app = new ApiFactory();
        using var client = app.Start();

        var response = await client.PostAsync("/user/register", Json(new { email, password, icon }));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await Body(response);
        Assert.Equal(400, problem.Value<int>("status"));
        Assert.Contains(expectedField, problem["errors"]!.ToString());
    }

    [Theory]
    [InlineData("{")]
    [InlineData("null")]
    [InlineData("{\"email\":\"valid@example.com\",\"password\":\"A-valid-password-123!\",\"user_id\":\"forged\"}")]
    public async Task Malformed_or_unknown_json_returns_bad_request(string json)
    {
        using var app = new ApiFactory();
        using var client = app.Start();

        var response = await client.PostAsync(
            "/user/register",
            new StringContent(json, Encoding.UTF8, "application/json"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Word_and_pagination_rules_are_enforced_on_create_and_update()
    {
        using var app = new ApiFactory();
        using var client = app.Start();
        Authorize(client, await Register(client, "validator@example.com"));

        var created = await Body(await client.PostAsync(
            "/dictionary", Json(new { dictionary_name = "English", language_id = "en" })));
        var dictionaryUrl = $"/dictionary/{created["id"]}";
        var wordsUrl = dictionaryUrl + "/word";

        var invalidDictionary = await client.PutAsync(
            dictionaryUrl, Json(new { dictionary_name = "English", language_id = "invalid language!" }));
        Assert.Equal(HttpStatusCode.BadRequest, invalidDictionary.StatusCode);

        foreach (var invalidWord in new object[]
        {
            new { original_word = " ", translated_word = "cat" },
            new { original_word = new string('a', 501), translated_word = "cat" },
            new { original_word = "cat", translated_word = new string('a', 2001) },
            new { original_word = "cat", translated_word = "cat", original_transcriptioned_word = new string('a', 501) }
        })
        {
            var response = await client.PostAsync(wordsUrl, Json(invalidWord));
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        var word = await Body(await client.PostAsync(
            wordsUrl, Json(new { original_word = "cat", translated_word = "кіт" })));
        var invalidUpdate = await client.PutAsync(
            $"{wordsUrl}/{word["id"]}", Json(new { original_word = "cat", translated_word = "" }));
        Assert.Equal(HttpStatusCode.BadRequest, invalidUpdate.StatusCode);

    }

    [Fact]
    public async Task Swagger_describes_all_fast_endpoints_and_snake_case_body_fields()
    {
        using var app = new ApiFactory();
        using var client = app.Start();
        var document = JObject.Parse(await client.GetStringAsync("/swagger/v1/swagger.json"));
        var paths = (JObject)document["paths"]!;

        Assert.Equal(16, paths.Count);
        foreach (var path in new[] { "/languages", "/dictionaries", "/users", "/user/activities",
            "/user/{id}/dictionaries", "/dictionary/{id}/words" })
        {
            var get = paths[path]!["get"]!;
            Assert.Null(get["requestBody"]);
            Assert.DoesNotContain(get["parameters"] ?? new JArray(), parameter => (string?)parameter["in"] == "query");
        }
        Assert.NotNull(paths["/languages"]!["get"]);
        Assert.Null(paths["/languages"]!["get"]!["requestBody"]);
        Assert.Empty(paths["/languages"]!["get"]!["parameters"] ?? new JArray());
        Assert.NotNull(paths["/user/activities"]!["get"]);
        Assert.NotNull(paths["/users"]!["get"]);
        Assert.NotNull(paths["/user/{id}/dictionaries"]!["get"]);
        Assert.NotNull(paths["/dictionary/order"]!["put"]);
        Assert.NotNull(paths["/language"]!["post"]);
        Assert.NotNull(paths["/language/{id}"]!["put"]);
        Assert.NotNull(paths["/user/{id}"]!["get"]);
        Assert.NotNull(paths["/dictionary/{id}/word/{wordId}"]!["put"]);
        Assert.NotNull(paths["/user/register"]!["post"]!["requestBody"]);
        Assert.NotNull(document["components"]!["schemas"]!["DictionaryRequest"]!["properties"]!["dictionary_name"]);
    }
}


