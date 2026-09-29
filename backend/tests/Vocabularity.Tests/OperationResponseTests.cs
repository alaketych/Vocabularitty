using System.Net;
using System.Text;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Vocabularity.Tests;

public sealed partial class ApiTests
{
    private static async Task AssertOperation(HttpResponseMessage response, HttpStatusCode status, bool success)
    {
        Assert.Equal(status, response.StatusCode);
        var body = await Body(response);
        Assert.Equal(new[] { "isSuccessfull", "message" }, body.Properties().Select(p => p.Name));
        Assert.Equal(success, body.Value<bool>("isSuccessfull"));
        Assert.False(string.IsNullOrWhiteSpace(body.Value<string>("message")));
    }

    [Fact]
    public async Task Resource_mutations_return_consistent_success_and_failure_envelopes()
    {
        using var app = new ApiFactory();
        using var client = app.Start();
        var request = new { dictionary_name = "English", language_id = "en" };
        await AssertOperation(await client.PostAsync("/dictionary", Json(request)), HttpStatusCode.Unauthorized, false);
        Authorize(client, await Register(client, "operation@example.com"));
        var created = await client.PostAsync("/dictionary", Json(request));
        await AssertOperation(created, HttpStatusCode.Created, true);
        Assert.NotNull(created.Headers.Location);
        var path = created.Headers.Location!.OriginalString;
        await AssertOperation(await client.PutAsync(path, Json(request)), HttpStatusCode.OK, true);
        await AssertOperation(await client.PostAsync("/dictionary", Json(new { dictionary_name = "", language_id = "en" })), HttpStatusCode.BadRequest, false);
        await AssertOperation(await client.PutAsync(path, new StringContent("{", Encoding.UTF8, "application/json")), HttpStatusCode.BadRequest, false);
        await AssertOperation(await client.PutAsync("/dictionary/missing", Json(request)), HttpStatusCode.NotFound, false);
        await AssertOperation(await client.PutAsync("/dictionary/order", Json(new { dictionary_ids = Array.Empty<string>() })), HttpStatusCode.Conflict, false);
        foreach (var (url, payload) in new (string, object)[]
        {
            ("/language", new { name = "English", original_name = "English" }),
            (path + "/word", new { original_word = "cat", translated_word = "кіт" })
        })
        {
            var result = await client.PostAsync(url, Json(payload));
            await AssertOperation(result, HttpStatusCode.Created, true);
            await AssertOperation(await client.PutAsync(result.Headers.Location, Json(payload)), HttpStatusCode.OK, true);
        }
        Authorize(client, await Register(client, "outsider-operation@example.com"));
        await AssertOperation(await client.PutAsync(path, Json(request)), HttpStatusCode.NotFound, false);
        var swagger = JObject.Parse(await client.GetStringAsync("/swagger/v1/swagger.json"));
        Assert.Contains("OperationResponse", swagger["paths"]!["/dictionary"]!["post"]!["responses"]!["400"]!.ToString());
    }
}
