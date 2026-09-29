using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vocabularity.Service.User.Entities;

namespace Vocabularity.Infrastructure.Database.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users", table =>
            table.HasCheckConstraint("CK_Users_Role", "[Role] IN ('User', 'Administrator')"));
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasMaxLength(36);
        builder.Property(x => x.Email).HasMaxLength(254).IsRequired();
        builder.Property(x => x.NormalizedEmail).HasMaxLength(254).IsRequired();
        builder.HasIndex(x => x.NormalizedEmail).IsUnique();
        builder.Property(x => x.PasswordHash).HasMaxLength(1024).IsRequired();
        builder.Property(x => x.Icon).HasMaxLength(2048);
        builder.Property(x => x.Role).HasMaxLength(20).HasDefaultValue("User").IsRequired();
    }
}

