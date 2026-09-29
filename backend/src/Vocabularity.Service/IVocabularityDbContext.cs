using Microsoft.EntityFrameworkCore;
using Vocabularity.Service.Dictionary.Entities;
using Vocabularity.Service.User.Entities;
using DictionaryEntity = Vocabularity.Service.Dictionary.Entities.Dictionary;

namespace Vocabularity.Service;

public interface IVocabularityDbContext
{
    DbSet<Language.Entities.Language> Languages { get; }

    DbSet<User.Entities.User> Users
    {
        get;
    }
    DbSet<DictionaryEntity> Dictionaries
    {
        get;
    }
    DbSet<Word> Words
    {
        get;
    }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction> BeginOrderingTransactionAsync(
        CancellationToken cancellationToken);
}

