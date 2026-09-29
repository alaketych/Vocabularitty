using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vocabularity.Service.Dictionary.Entities;
using DictionaryEntity = Vocabularity.Service.Dictionary.Entities.Dictionary;

namespace Vocabularity.Infrastructure.Database.Configurations;

public sealed class WordConfiguration : IEntityTypeConfiguration<Word>
{
    public void Configure(EntityTypeBuilder<Word> builder)
    {
        builder.ToTable("Words");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasMaxLength(36);
        builder.Property(x => x.DictionaryId).HasMaxLength(36).IsRequired();
        builder.Property(x => x.OriginalWord).HasMaxLength(500).IsRequired();
        builder.Property(x => x.OriginalTranscriptionedWord).HasMaxLength(500);
        builder.Property(x => x.TranslatedWord).HasMaxLength(2000).IsRequired();
        builder.HasOne<DictionaryEntity>().WithMany().HasForeignKey(x => x.DictionaryId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(x => new { x.DictionaryId, x.CreatedAt });
    }
}

