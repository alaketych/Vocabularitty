using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Vocabularity.Service.User.Entities;
using Vocabularity.Service.User;
using Vocabularity.Service.Dictionary.Entities;
using Language = Vocabularity.Service.Language.Entities.Language;
using DictionaryEntity = Vocabularity.Service.Dictionary.Entities.Dictionary;

namespace Vocabularity.Infrastructure.Database;

public static class DemoData
{
    public const string AdminId = "00000000-0000-0000-0000-000000000001";
    public const string AdminEmail = "admin@vocabularity.test";

    public static TBuilder UseDemoData<TBuilder>(this TBuilder options, bool enabled) where TBuilder : DbContextOptionsBuilder
    {
        if (!enabled) return options;
        options.UseSeeding((context, _) => Seed((VocabularityDbContext)context))
            .UseAsyncSeeding((context, _, cancellationToken) =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                Seed((VocabularityDbContext)context);
                return Task.CompletedTask;
            });
        return options;
    }

    // Fixed IDs and existence checks keep repeated migrations/startups idempotent.
    // Existing demo edits/passwords are preserved; only missing fixtures are inserted.
    public static void Seed(VocabularityDbContext database)
    {
        var admin = database.Users.SingleOrDefault(user => user.Id == AdminId);
        if (admin is null)
        {
            if (database.Users.Any(user => user.NormalizedEmail == AdminEmail.ToUpperInvariant()))
                throw new InvalidOperationException("Demo admin email is already used by another account.");
            admin = new User { Id = AdminId, Email = AdminEmail,
                NormalizedEmail = AdminEmail.ToUpperInvariant(), PasswordHash = "", Role = UserRoles.Administrator };
            admin.PasswordHash = new PasswordHasher<User>().HashPassword(admin, "admin");
            database.Users.Add(admin);
            database.SaveChanges();
        }

        var samples = new[]
        {
            (Code: "en", Name: "English", Native: "English", Flag: "🇬🇧", Words: new[] {
                ("hello", "həˈləʊ", "привіт"), ("book", "bʊk", "книга"), ("friend", "frend", "друг") }),
            (Code: "es", Name: "Spanish", Native: "Español", Flag: "🇪🇸", Words: new[] {
                ("hola", "ˈola", "привіт"), ("libro", "ˈliβɾo", "книга"), ("amigo", "aˈmiɣo", "друг") }),
            (Code: "fr", Name: "French", Native: "Français", Flag: "🇫🇷", Words: new[] {
                ("bonjour", "bɔ̃ʒuʁ", "привіт"), ("livre", "livʁ", "книга"), ("ami", "ami", "друг") })
        };
        for (var index = 0; index < samples.Length; index++)
        {
            var sample = samples[index];
            if (!database.Languages.Any(language => language.Id == sample.Code) &&
                !database.Languages.Any(language => language.Name == sample.Name))
                database.Languages.Add(new Language { Id = sample.Code, Name = sample.Name, OriginalName = sample.Native, Icon = sample.Flag });
            var dictionaryId = $"demo-dictionary-{sample.Code}";
            if (!database.Dictionaries.Any(dictionary => dictionary.Id == dictionaryId))
                database.Dictionaries.Add(new DictionaryEntity { Id = dictionaryId, UserId = AdminId,
                    Name = $"{sample.Name} essentials", LanguageId = sample.Code, Position = index });
            for (var wordIndex = 0; wordIndex < sample.Words.Length; wordIndex++)
            {
                var wordId = $"demo-{sample.Code}-word-{wordIndex}";
                var (original, transcription, translation) = sample.Words[wordIndex];
                if (!database.Words.Any(word => word.Id == wordId))
                    database.Words.Add(new Word { Id = wordId, DictionaryId = dictionaryId,
                        OriginalWord = original, OriginalTranscriptionedWord = transcription, TranslatedWord = translation });
            }
        }
        database.SaveChanges();
    }
}
