using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using Vocabularity.Infrastructure.Database;
using Xunit;

namespace Vocabularity.Tests;

public sealed class SqlServerFactAttribute : FactAttribute
{
    public SqlServerFactAttribute()
    {
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("VOCABULARITY_TEST_SQLSERVER")))
        {
            Skip = "Set VOCABULARITY_TEST_SQLSERVER to verify the SQL Server data migration.";
        }
    }
}

public sealed partial class ApiTests
{
    [SqlServerFact]
    public async Task Migration_backfills_existing_dictionaries_per_user_in_previous_list_order()
    {
        using var app = new ApiFactory();
        using var client = app.Start();
        Authorize(client, await Register(client, "migration-one@example.com"));
        await CreateDictionary(client, "First");
        await CreateDictionary(client, "Second");
        Authorize(client, await Register(client, "migration-two@example.com"));
        await CreateDictionary(client, "Other user's first");

        using var scope = app.Services.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<VocabularityDbContext>();
        var before = await database.Dictionaries.AsNoTracking()
            .OrderBy(dictionary => dictionary.CreatedAt)
            .ThenBy(dictionary => dictionary.Id)
            .ToListAsync();

        // Recreate the actual pre-feature schema, keeping all existing rows.
        var migrator = database.GetService<IMigrator>();
        await migrator.MigrateAsync("20260923134437_AddLanguages");
        await migrator.MigrateAsync();

        Assert.All(await database.Users.AsNoTracking().ToListAsync(),
            user => Assert.Equal("User", user.Role));

        var after = await database.Dictionaries.AsNoTracking().ToDictionaryAsync(dictionary => dictionary.Id);
        foreach (var owner in before.GroupBy(dictionary => dictionary.UserId))
        {
            var position = 0;
            foreach (var dictionary in owner)
            {
                Assert.Equal(position++, after[dictionary.Id].Position);
                Assert.Equal(dictionary.Name, after[dictionary.Id].Name);
            }
        }
    }
}

