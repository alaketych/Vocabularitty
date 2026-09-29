using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Vocabularity.Core;
using Vocabularity.Service;
using Vocabularity.Service.Dictionary.Entities;
using Vocabularity.Service.User.Entities;
using DictionaryEntity = Vocabularity.Service.Dictionary.Entities.Dictionary;

namespace Vocabularity.Infrastructure.Database;

public sealed class VocabularityDbContext(DbContextOptions<VocabularityDbContext> options,
    Vocabularity.Service.Activity.ICurrentActor? actor = null) : DbContext(options), IVocabularityDbContext
{
    public DbSet<Vocabularity.Service.Activity.Entities.UserActivity> UserActivities => Set<Vocabularity.Service.Activity.Entities.UserActivity>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Vocabularity.Service.Language.Entities.Language> Languages =>
        Set<Vocabularity.Service.Language.Entities.Language>();
    public DbSet<DictionaryEntity> Dictionaries => Set<DictionaryEntity>();
    public DbSet<Word> Words => Set<Word>();
    protected override void OnModelCreating(ModelBuilder model)
    {
        model.ApplyConfigurationsFromAssembly(typeof(VocabularityDbContext).Assembly);
    }

    public Task<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction> BeginOrderingTransactionAsync(
        CancellationToken cancellationToken) =>
        Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, cancellationToken);

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ChangeTracker.DetectChanges();
        var activities = new List<Vocabularity.Service.Activity.Entities.UserActivity>();
        foreach (var entry in ChangeTracker.Entries<Entity>().ToList())
        {
            if (entry.Entity is Vocabularity.Service.Activity.Entities.UserActivity ||
                entry.State is not (EntityState.Added or EntityState.Modified or EntityState.Deleted)) continue;
            var userId = actor?.UserId ?? (entry.Entity is User user ? user.Id : null);
            if (userId is null) continue; // No user actor for maintenance/design-time operations.
            var entity = entry.Entity switch
            {
                DictionaryEntity => "dictionary", Word => "word", User => "user",
                Vocabularity.Service.Language.Entities.Language => "language", _ => null
            };
            if (entity is null) continue;
            var action = entry.State switch
            {
                EntityState.Added => "create", EntityState.Deleted => "delete", _ => "update"
            };
            if (entry.Entity is DictionaryEntity && entry.State == EntityState.Modified &&
                entry.Property(nameof(DictionaryEntity.Position)).IsModified) action = "reorder";
            activities.Add(new() { UserId = userId, Function = $"{action}_{entity}", EntityId = entry.Entity.Id });
        }
        UserActivities.AddRange(activities);
        try
        {
            return await base.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException e) when (e.InnerException is SqlException { Number: 2601 or 2627 })
        {
            foreach (var activity in activities) Entry(activity).State = EntityState.Detached;
            throw new ApiException(409, "A record with this unique value already exists.");
        }
        catch (DbUpdateConcurrencyException)
        {
            foreach (var activity in activities) Entry(activity).State = EntityState.Detached;
            throw new ApiException(409, "The record changed. Refresh and retry.");
        }
        catch
        {
            foreach (var activity in activities) Entry(activity).State = EntityState.Detached;
            throw;
        }
    }
}

