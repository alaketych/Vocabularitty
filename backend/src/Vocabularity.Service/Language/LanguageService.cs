using Microsoft.EntityFrameworkCore;
using Vocabularity.Core;
using Vocabularity.Service.Language.Models;
using LanguageEntity = Vocabularity.Service.Language.Entities.Language;

namespace Vocabularity.Service.Language;

public sealed class LanguageService(IVocabularityDbContext database)
{
    public async Task<IReadOnlyList<LanguageResponse>> ListAsync(
        CancellationToken cancellationToken)
    {
        return await database.Languages
            .AsNoTracking()
            .OrderBy(language => language.Name)
            .ThenBy(language => language.Id)
            .Select(language => new LanguageResponse(
                language.Id, language.Name, language.OriginalName, language.Icon))
            .ToListAsync(cancellationToken);
    }

    public async Task<LanguageResponse> GetAsync(string id, CancellationToken cancellationToken)
    {
        var language = await FindAsync(id, cancellationToken);
        return Map(language);
    }

    public async Task<LanguageResponse> CreateAsync(
        LanguageRequest request,
        CancellationToken cancellationToken)
    {
        var language = new LanguageEntity
        {
            Name = request.Name.Trim(),
            OriginalName = request.OriginalName.Trim(),
            Icon = NormalizeIcon(request.Icon)
        };

        database.Languages.Add(language);
        await database.SaveChangesAsync(cancellationToken);

        return Map(language);
    }

    public async Task<LanguageResponse> UpdateAsync(
        string id,
        LanguageRequest request,
        CancellationToken cancellationToken)
    {
        var language = await FindAsync(id, cancellationToken);

        language.Name = request.Name.Trim();
        language.OriginalName = request.OriginalName.Trim();
        language.Icon = NormalizeIcon(request.Icon);
        language.UpdatedAt = DateTime.UtcNow;

        await database.SaveChangesAsync(cancellationToken);
        return Map(language);
    }

    public async Task DeleteAsync(string id, CancellationToken cancellationToken)
    {
        var language = await FindAsync(id, cancellationToken);

        database.Languages.Remove(language);
        await database.SaveChangesAsync(cancellationToken);
    }

    private async Task<LanguageEntity> FindAsync(string id, CancellationToken cancellationToken)
    {
        return await database.Languages.SingleOrDefaultAsync(
            language => language.Id == id, cancellationToken)
            ?? throw new ApiException(404, "Language not found.");
    }

    private static string? NormalizeIcon(string? icon) =>
        string.IsNullOrWhiteSpace(icon) ? null : icon.Trim();

    private static LanguageResponse Map(LanguageEntity language) =>
        new(language.Id, language.Name, language.OriginalName, language.Icon);
}


