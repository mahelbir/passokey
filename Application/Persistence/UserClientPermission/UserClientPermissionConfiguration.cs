using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Application.Persistence.UserClientPermission;

public class UserClientPermissionConfiguration : IEntityTypeConfiguration<UserClientPermissionEntity>
{
    public void Configure(EntityTypeBuilder<UserClientPermissionEntity> builder)
    {
        builder.ToTable("UserClientPermissions");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.UserId).IsRequired();
        builder.Property(e => e.ClientId).IsRequired();
        builder.HasIndex(e => new { e.UserId, e.ClientId }).IsUnique();

        builder.HasOne(e => e.User)
            .WithMany(u => u.ClientPermissions)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Client)
            .WithMany(c => c.UserPermissions)
            .HasForeignKey(e => e.ClientId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
