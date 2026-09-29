using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vocabularity.Service.Activity.Entities;
using Vocabularity.Service.User.Entities;

namespace Vocabularity.Infrastructure.Database.Configurations;

public sealed class UserActivityConfiguration : IEntityTypeConfiguration<UserActivity>
{
    public void Configure(EntityTypeBuilder<UserActivity> builder)
    {
        builder.ToTable("UserActivities");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasMaxLength(36);
        builder.Property(x => x.UserId).HasMaxLength(36).IsRequired();
        builder.Property(x => x.Function).HasMaxLength(64).IsRequired();
        builder.Property(x => x.EntityId).HasMaxLength(36).IsRequired();
        builder.Property(x => x.Date).HasConversion(
            value => value, value => DateTime.SpecifyKind(value, DateTimeKind.Utc));
        builder.HasIndex(x => new { x.UserId, x.Date, x.Id });
        builder.HasOne<User>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
    }
}
