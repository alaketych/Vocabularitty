using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vocabularity.Service.User.Entities;
using DictionaryEntity = Vocabularity.Service.Dictionary.Entities.Dictionary;

namespace Vocabularity.Infrastructure.Database.Configurations;

public sealed class DictionaryConfiguration : IEntityTypeConfiguration<DictionaryEntity>
{
    public void Configure(EntityTypeBuilder<DictionaryEntity> builder)
    {
        builder.ToTable("Dictionaries");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasMaxLength(36);
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.UserId).HasMaxLength(36).IsRequired();
        builder.Property(x => x.LanguageId).HasMaxLength(35).IsRequired();
        builder.HasOne<User>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(x => new { x.UserId, x.CreatedAt });
        builder.HasIndex(x => new { x.UserId, x.Position });
    }
}

