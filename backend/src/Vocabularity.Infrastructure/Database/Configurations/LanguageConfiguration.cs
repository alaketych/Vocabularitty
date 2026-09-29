using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vocabularity.Service.Language.Entities;

namespace Vocabularity.Infrastructure.Database.Configurations;

public sealed class LanguageConfiguration : IEntityTypeConfiguration<Language>
{
    public void Configure(EntityTypeBuilder<Language> builder)
    {
        builder.ToTable("Languages");
        builder.HasKey(language => language.Id);
        builder.Property(language => language.Id).HasMaxLength(36);
        builder.Property(language => language.Name).HasMaxLength(100).IsRequired();
        builder.Property(language => language.OriginalName).HasMaxLength(100).IsRequired();
        builder.Property(language => language.Icon).HasMaxLength(2048);
        builder.HasIndex(language => language.Name);
    }
}


