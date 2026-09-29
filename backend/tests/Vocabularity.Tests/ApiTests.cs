using System.Net;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Vocabularity.Infrastructure.Database;
using Xunit;

namespace Vocabularity.Tests;

// Each factory owns a new database. SQL mode never uses or deletes the supplied database.
public sealed class ApiFactory : WebApplicationFactory<Program>
{
    private bool cleanedUp;
    private readonly SqliteConnection sqlite = new("Data Source=:memory:");
    private readonly string? sql = Environment.GetEnvironmentVariable("VOCABULARITY_TEST_SQLSERVER");
    private readonly bool demo;
    public ApiFactory(bool demo = false) { this.demo = demo; sqlite.Open(); }
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureAppConfiguration((_, c) => c.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Demo:Enabled"] = demo.ToString(),
            ["Demo:InitializeDatabase"] = "false",
            ["Jwt:Key"] = "tests-only-random-looking-signing-key-at-least-32-bytes",
            ["Jwt:Issuer"] = "Vocabularity",
            ["Jwt:Audience"] = "Vocabularity.Client"
        }));
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<VocabularityDbContext>>();
            services.RemoveAll<Microsoft.EntityFrameworkCore.Infrastructure.IDbContextOptionsConfiguration<VocabularityDbContext>>();
            if (sql is null)
                services.AddDbContext<VocabularityDbContext>(o => o.UseSqlite(sqlite));
            else
            {
                var connection = new SqlConnectionStringBuilder(sql) { InitialCatalog = "VocabularityTests_" + Guid.NewGuid().ToString("N") };
                services.AddDbContext<VocabularityDbContext>(o => o.UseSqlServer(connection.ConnectionString));
            }
        });
    }
    public HttpClient Start()
    {
        var client = CreateClient();
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<VocabularityDbContext>();
        if (sql is null)
            db.Database.EnsureCreated();
        else
            db.Database.Migrate();
        return client;
    }
    protected override void Dispose(bool disposing)
    {
        if (disposing && !cleanedUp)
        {
            cleanedUp = true;
            using var scope = Services.CreateScope();
            scope.ServiceProvider.GetRequiredService<VocabularityDbContext>().Database.EnsureDeleted();
            sqlite.Dispose();
        }
        base.Dispose(disposing);
    }
}
public sealed partial class ApiTests
{
    private static StringContent Json(object value) => new(JsonConvert.SerializeObject(value), Encoding.UTF8, "application/json");
    private static async Task<JObject> Body(HttpResponseMessage response) => JObject.Parse(await response.Content.ReadAsStringAsync());
    private static async Task<string> Register(HttpClient client, string email)
    {
        var response = await client.PostAsync("/user/register", Json(new
        {
            email,
            password = "Test-password-12345!"
        }));
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await Body(response);
        Assert.Null(body["user"]!["password_hash"]);
        return body.Value<string>("access_token")!;
    }
    private static void Authorize(HttpClient c, string token) => c.DefaultRequestHeaders.Authorization = new("Bearer", token);
    [Fact]
    public async Task Register_login_hash_and_normalized_unique_email()
    {
        using var app = new ApiFactory();
        using var client = app.Start();
        await Register(client, "Alice@example.com");
        var duplicate = await client.PostAsync("/user/register", Json(new
        {
            email = "ALICE@example.com",
            password = "Test-password-12345!"
        }));
        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);
        var login = await client.PostAsync("/user/login", Json(new
        {
            email = "alice@example.com",
            password = "Test-password-12345!"
        }));
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        var bad = await client.PostAsync("/user/login", Json(new
        {
            email = "alice@example.com",
            password = "wrong"
        }));
        Assert.Equal(HttpStatusCode.Unauthorized, bad.StatusCode);
        using var scope = app.Services.CreateScope();
        var user = await scope.ServiceProvider.GetRequiredService<VocabularityDbContext>().Users.SingleAsync();
        Assert.NotEqual("Test-password-12345!", user.PasswordHash);
    }
    [Fact]
    public async Task Full_crud_and_cascade_delete()
    {
        using var app = new ApiFactory();
        using var client = app.Start();
        Authorize(client, await Register(client, "owner@example.com"));
        var created = await client.PostAsync("/dictionary", Json(new
        {
            dictionary_name = "English",
            language_id = "en"
        }));
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var path = "/dictionary/" + (await Body(created)).Value<string>("id");
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync(path)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/dictionaries")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.PutAsync(path, Json(new
        {
            dictionary_name = "Updated",
            language_id = "en-GB"
        }))).StatusCode);
        var word = await client.PostAsync(path + "/word", Json(new
        {
            original_word = "cat",
            translated_word = "кіт"
        }));
        Assert.Equal(HttpStatusCode.Created, word.StatusCode);
        var wordPath = path + "/word/" + (await Body(word)).Value<string>("id");
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync(wordPath)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync(path + "/words")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.PutAsync(wordPath, Json(new
        {
            original_word = "dog",
            translated_word = "пес",
            original_transcriptioned_word = "dɒɡ"
        }))).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await client.DeleteAsync(wordPath)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync(wordPath)).StatusCode);
        await client.PostAsync(path + "/word", Json(new
        {
            original_word = "house",
            translated_word = "дім"
        }));
        Assert.Equal(HttpStatusCode.NoContent, (await client.DeleteAsync(path)).StatusCode);
        using var scope = app.Services.CreateScope();
        Assert.Empty(await scope.ServiceProvider.GetRequiredService<VocabularityDbContext>().Words.ToListAsync());
    }
    [Fact]
    public async Task Other_users_cannot_access_any_dictionary_or_word_operation()
    {
        using var app = new ApiFactory();
        using var client = app.Start();
        var owner = await Register(client, "owner@example.com");
        var stranger = await Register(client, "stranger@example.com");
        Authorize(client, owner);
        var dictionary = await client.PostAsync("/dictionary", Json(new
        {
            dictionary_name = "Private",
            language_id = "en"
        }));
        var path = "/dictionary/" + (await Body(dictionary)).Value<string>("id");
        var word = await client.PostAsync(path + "/word", Json(new
        {
            original_word = "cat",
            translated_word = "кіт"
        }));
        var wordPath = path + "/word/" + (await Body(word)).Value<string>("id");
        Authorize(client, stranger);
        Assert.Equal("[]", await client.GetStringAsync("/dictionaries"));
        foreach (var url in new[] { path, path + "/words", wordPath })
            Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync(url)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.PutAsync(path, Json(new
        {
            dictionary_name = "Stolen",
            language_id = "en"
        }))).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.DeleteAsync(path)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.PostAsync(path + "/word", Json(new
        {
            original_word = "x",
            translated_word = "y"
        }))).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.PutAsync(wordPath, Json(new
        {
            original_word = "x",
            translated_word = "y"
        }))).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.DeleteAsync(wordPath)).StatusCode);
    }
    [Fact]
    public async Task Validation_authentication_and_swagger_contract()
    {
        using var app = new ApiFactory();
        using var client = app.Start();
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/dictionaries")).StatusCode);
        Authorize(client, "invalid.token.value");
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/dictionaries")).StatusCode);
        client.DefaultRequestHeaders.Authorization = null;
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsync("/user/register", Json(new
        {
            email = "bad",
            password = "short"
        }))).StatusCode);
        Authorize(client, await Register(client, "valid@example.com"));
        foreach (var data in new object[] {
            new { dictionary_name = " ", language_id = "en" },
            new { dictionary_name = "Name", language_id = "en", user_id = "forged" } })
            Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsync("/dictionary", Json(data))).StatusCode);
        var swagger = await client.GetStringAsync("/swagger/v1/swagger.json");
        Assert.Contains("dictionary_name", swagger);
        Assert.Contains("Bearer", swagger);
        Assert.DoesNotContain("password_hash", swagger);
    }
    [Fact]
    public async Task Words_cannot_be_addressed_through_another_owned_dictionary()
    {
        using var app = new ApiFactory();
        using var client = app.Start();
        Authorize(client, await Register(client, "owner@example.com"));
        var a = await Body(await client.PostAsync("/dictionary", Json(new
        {
            dictionary_name = "A",
            language_id = "en"
        })));
        var b = await Body(await client.PostAsync("/dictionary", Json(new
        {
            dictionary_name = "B",
            language_id = "de"
        })));
        var word = await Body(await client.PostAsync($"/dictionary/{a["id"]}/word", Json(new
        {
            original_word = "cat",
            translated_word = "кіт"
        })));
        var wrong = $"/dictionary/{b["id"]}/word/{word["id"]}";
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync(wrong)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.PutAsync(wrong, Json(new
        {
            original_word = "x",
            translated_word = "y"
        }))).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.DeleteAsync(wrong)).StatusCode);
    }
}


