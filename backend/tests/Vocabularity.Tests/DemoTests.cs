using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Vocabularity.Infrastructure.Database;
using Xunit;

namespace Vocabularity.Tests;

public sealed partial class ApiTests
{
    [Fact]
    public async Task Demo_seed_is_repeatable_and_anonymous_reads_work_but_writes_require_login()
    {
        using var app = new ApiFactory(demo: true);
        using var client = app.Start();
        using (var scope = app.Services.CreateScope())
        {
            var database = scope.ServiceProvider.GetRequiredService<VocabularityDbContext>();
            DemoData.Seed(database);
            DemoData.Seed(database);
            Assert.Equal(1, database.Users.Count());
            Assert.Equal(3, database.Languages.Count());
            Assert.Equal(3, database.Dictionaries.Count());
            Assert.Equal(9, database.Words.Count());
        }
        foreach (var path in new[] { "/users", $"/user/{DemoData.AdminId}", "/languages", "/language/en",
            "/dictionaries", "/dictionary/demo-dictionary-en", "/dictionary/demo-dictionary-en/words",
            "/dictionary/demo-dictionary-en/word/demo-en-word-0", $"/user/{DemoData.AdminId}/dictionaries" })
            Assert.Equal(HttpStatusCode.OK, (await client.GetAsync(path)).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.DeleteAsync("/dictionary/demo-dictionary-en")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/user/activities")).StatusCode);
        var login = await client.PostAsync("/user/login", Json(new { username = "admin", password = "admin" }));
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        var response = await Body(login);
        Assert.Equal("Administrator", (string?)response["user"]?["role"]);
        Assert.DoesNotContain("password_hash", await client.GetStringAsync("/users"));
    }
}
