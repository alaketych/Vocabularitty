using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Vocabularity.Infrastructure.Database;
using Xunit;

namespace Vocabularity.Tests;

public sealed partial class ApiTests
{
    [Fact]
    public async Task Pagination_defaults_boundaries_and_error_contract()
    {
        using var app = new ApiFactory(demo: true);
        using var client = app.Start();
        using (var scope = app.Services.CreateScope())
            DemoData.Seed(scope.ServiceProvider.GetRequiredService<VocabularityDbContext>());
        var defaults = await Body(await client.GetAsync("/dictionaries"));
        Assert.Equal(1, defaults.Value<int>("pageNumber"));
        Assert.Equal(12, defaults.Value<int>("pageSize"));
        Assert.Equal(3, defaults["data"]!.Count());
        var first = await Body(await client.GetAsync("/dictionaries?pageNumber=1&pageSize=2"));
        var second = await Body(await client.GetAsync("/dictionaries?pageNumber=2&pageSize=2"));
        Assert.Equal(2, first["data"]!.Count());
        Assert.Single(second["data"]!);
        Assert.DoesNotContain((string?)second["data"]![0]!["id"], first["data"]!.Select(item => (string?)item["id"]));
        Assert.Empty(ReadPage(await client.GetStringAsync("/dictionaries?pageNumber=3&pageSize=2")));
        foreach (var query in new[] { "pageNumber=0", "pageSize=0", "pageSize=101", "pageNumber=abc", "pageNumber=2147483647&pageSize=100" })
        {
            var response = await client.GetAsync("/languages?" + query);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull((await Body(response))["ErrorMessage"]);
        }
        var denied = await client.DeleteAsync("/dictionary/demo-dictionary-en");
        Assert.Equal(HttpStatusCode.Unauthorized, denied.StatusCode);
        Assert.NotNull((await Body(denied))["ErrorMessage"]);
        var missing = await client.GetAsync("/dictionary/missing");
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
        Assert.Contains("permission", (string)(await Body(missing))["ErrorMessage"]!);
    }
}
