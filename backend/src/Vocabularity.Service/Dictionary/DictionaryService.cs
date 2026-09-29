using Microsoft.EntityFrameworkCore;
using Vocabularity.Core;
using Vocabularity.Service.Dictionary.Entities;
using Vocabularity.Service.Dictionary.Models;
using DictionaryEntity = Vocabularity.Service.Dictionary.Entities.Dictionary;

namespace Vocabularity.Service.Dictionary;

public sealed class DictionaryService(IVocabularityDbContext database)
{
    public async Task<IReadOnlyList<DictionaryResponse>> ListAsync(
        string userId,
        CancellationToken cancellationToken,
        bool isAdministrator = false)
    {
        return await database.Dictionaries
            .AsNoTracking()
            .Where(dictionary => isAdministrator || dictionary.UserId == userId)
            .OrderBy(dictionary => dictionary.Position)
            .ThenBy(dictionary => dictionary.CreatedAt)
            .ThenBy(dictionary => dictionary.Id)
            .Select(dictionary => new DictionaryResponse(
                dictionary.Id, dictionary.Name, dictionary.UserId, dictionary.LanguageId, dictionary.Position))
            .ToListAsync(cancellationToken);
    }

    public async Task<DictionaryResponse> GetAsync(
        string userId,
        string dictionaryId,
        CancellationToken cancellationToken,
        bool isAdministrator = false)
    {
        var dictionary = await FindAccessibleDictionaryAsync(userId, dictionaryId, cancellationToken, isAdministrator);
        return MapDictionary(dictionary);
    }

    public async Task<DictionaryResponse> CreateAsync(
        string userId,
        DictionaryRequest request,
        CancellationToken cancellationToken)
    {
        await using var transaction = await database.BeginOrderingTransactionAsync(cancellationToken);
        var lastPosition = await database.Dictionaries
            .Where(dictionary => dictionary.UserId == userId)
            .MaxAsync(dictionary => (int?)dictionary.Position, cancellationToken);

        var dictionary = new DictionaryEntity
        {
            Name = request.Name.Trim(),
            LanguageId = request.LanguageId,
            UserId = userId,
            Position = checked((lastPosition ?? -1) + 1)
        };

        database.Dictionaries.Add(dictionary);
        await database.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return MapDictionary(dictionary);
    }

    public async Task ReorderAsync(
        string userId,
        ReorderDictionariesRequest request,
        CancellationToken cancellationToken)
    {
        // Serialize the read and write so concurrent creates cannot change the set mid-reorder.
        await using var transaction = await database.BeginOrderingTransactionAsync(cancellationToken);
        var dictionaries = await database.Dictionaries
            .Where(dictionary => dictionary.UserId == userId)
            .ToListAsync(cancellationToken);
        var requestedIds = request.DictionaryIds;
        var ownedIds = dictionaries.Select(dictionary => dictionary.Id).ToHashSet(StringComparer.Ordinal);

        if (requestedIds.Count != ownedIds.Count || !ownedIds.SetEquals(requestedIds))
        {
            throw new ApiException(409, "Send every current dictionary ID exactly once. Refresh the list and retry.");
        }

        var byId = dictionaries.ToDictionary(dictionary => dictionary.Id, StringComparer.Ordinal);
        var updatedAt = DateTime.UtcNow;

        for (var position = 0; position < requestedIds.Count; position++)
        {
            var dictionary = byId[requestedIds[position]];
            dictionary.Position = position;
            dictionary.UpdatedAt = updatedAt;
        }

        await database.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    public async Task<DictionaryResponse> UpdateAsync(
        string userId,
        string dictionaryId,
        DictionaryRequest request,
        CancellationToken cancellationToken)
    {
        var dictionary = await FindAccessibleDictionaryAsync(userId, dictionaryId, cancellationToken);

        dictionary.Name = request.Name.Trim();
        dictionary.LanguageId = request.LanguageId;
        dictionary.UpdatedAt = DateTime.UtcNow;

        await database.SaveChangesAsync(cancellationToken);
        return MapDictionary(dictionary);
    }

    public async Task DeleteAsync(
        string userId,
        string dictionaryId,
        CancellationToken cancellationToken)
    {
        var dictionary = await FindAccessibleDictionaryAsync(userId, dictionaryId, cancellationToken);

        database.Dictionaries.Remove(dictionary);
        await database.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<WordResponse>> ListWordsAsync(
        string userId,
        string dictionaryId,
        CancellationToken cancellationToken,
        bool isAdministrator = false)
    {
        await FindAccessibleDictionaryAsync(userId, dictionaryId, cancellationToken, isAdministrator);

        return await database.Words
            .AsNoTracking()
            .Where(word => word.DictionaryId == dictionaryId)
            .OrderBy(word => word.CreatedAt)
            .ThenBy(word => word.Id)
            .Select(word => new WordResponse(
                word.Id,
                word.DictionaryId,
                word.OriginalWord,
                word.OriginalTranscriptionedWord,
                word.TranslatedWord))
            .ToListAsync(cancellationToken);
    }

    public async Task<WordResponse> GetWordAsync(
        string userId,
        string dictionaryId,
        string wordId,
        CancellationToken cancellationToken,
        bool isAdministrator = false)
    {
        var word = await FindAccessibleWordAsync(userId, dictionaryId, wordId, cancellationToken, isAdministrator);
        return MapWord(word);
    }

    public async Task<WordResponse> CreateWordAsync(
        string userId,
        string dictionaryId,
        WordRequest request,
        CancellationToken cancellationToken)
    {
        await FindAccessibleDictionaryAsync(userId, dictionaryId, cancellationToken);

        var word = new Word
        {
            DictionaryId = dictionaryId,
            OriginalWord = request.OriginalWord.Trim(),
            OriginalTranscriptionedWord = request.OriginalTranscriptionedWord,
            TranslatedWord = request.TranslatedWord.Trim()
        };

        database.Words.Add(word);
        await database.SaveChangesAsync(cancellationToken);

        return MapWord(word);
    }

    public async Task<WordResponse> UpdateWordAsync(
        string userId,
        string dictionaryId,
        string wordId,
        WordRequest request,
        CancellationToken cancellationToken)
    {
        var word = await FindAccessibleWordAsync(userId, dictionaryId, wordId, cancellationToken);

        word.OriginalWord = request.OriginalWord.Trim();
        word.OriginalTranscriptionedWord = request.OriginalTranscriptionedWord;
        word.TranslatedWord = request.TranslatedWord.Trim();
        word.UpdatedAt = DateTime.UtcNow;

        await database.SaveChangesAsync(cancellationToken);
        return MapWord(word);
    }

    public async Task DeleteWordAsync(
        string userId,
        string dictionaryId,
        string wordId,
        CancellationToken cancellationToken)
    {
        var word = await FindAccessibleWordAsync(userId, dictionaryId, wordId, cancellationToken);

        database.Words.Remove(word);
        await database.SaveChangesAsync(cancellationToken);
    }

    private async Task<DictionaryEntity> FindAccessibleDictionaryAsync(
        string userId,
        string dictionaryId,
        CancellationToken cancellationToken,
        bool isAdministrator = false)
    {
        return await database.Dictionaries.SingleOrDefaultAsync(
            dictionary => dictionary.Id == dictionaryId && (isAdministrator || dictionary.UserId == userId),
            cancellationToken)
            ?? throw new ApiException(404, "Dictionary not found.");
    }

    private async Task<Word> FindAccessibleWordAsync(
        string userId,
        string dictionaryId,
        string wordId,
        CancellationToken cancellationToken,
        bool isAdministrator = false)
    {
        // Check access before looking up a word, including when its ID is known.
        await FindAccessibleDictionaryAsync(userId, dictionaryId, cancellationToken, isAdministrator);

        return await database.Words.SingleOrDefaultAsync(
            word => word.Id == wordId && word.DictionaryId == dictionaryId,
            cancellationToken)
            ?? throw new ApiException(404, "Word not found.");
    }

    private static DictionaryResponse MapDictionary(DictionaryEntity dictionary) =>
        new(dictionary.Id, dictionary.Name, dictionary.UserId, dictionary.LanguageId, dictionary.Position);

    private static WordResponse MapWord(Word word) =>
        new(word.Id, word.DictionaryId, word.OriginalWord,
            word.OriginalTranscriptionedWord, word.TranslatedWord);
}




