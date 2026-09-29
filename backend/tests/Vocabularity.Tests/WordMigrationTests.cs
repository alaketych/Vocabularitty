using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using Vocabularity.Infrastructure.Database;
using Xunit;

namespace Vocabularity.Tests;

public sealed partial class ApiTests
{
    [SqlServerFact]
    public async Task Word_table_rename_preserves_data_and_cascade_delete()
    {
        using var app = new ApiFactory();
        using var client = app.Start();
        Authorize(client, await Register(client, "word-migration@example.com"));
        var dictionaryId = await CreateDictionary(client, "Migration words");
        var created = await client.PostAsync($"/dictionary/{dictionaryId}/word", Json(new
        {
            original_word = "cat",
            original_transcriptioned_word = "kæt",
            translated_word = "кіт"
        }));
        Assert.Equal(System.Net.HttpStatusCode.Created, created.StatusCode);
        var wordId = (await Body(created)).Value<string>("id");

        using var scope = app.Services.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<VocabularityDbContext>();
        var migrator = database.GetService<IMigrator>();

        // Exercise both directions with a populated table in this isolated test database.
        await migrator.MigrateAsync("20260924124737_AddDictionaryPosition");
        await migrator.MigrateAsync();

        var word = await database.Words.AsNoTracking().SingleAsync();
        Assert.Equal(wordId, word.Id);
        Assert.Equal(dictionaryId, word.DictionaryId);
        Assert.Equal("cat", word.OriginalWord);
        Assert.Equal("kæt", word.OriginalTranscriptionedWord);
        Assert.Equal("кіт", word.TranslatedWord);

        await client.DeleteAsync($"/dictionary/{dictionaryId}");
        Assert.Empty(await database.Words.AsNoTracking().ToListAsync());
    }
}
