using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Application.Persistence.UserCredential;

public class UserCredentialConfiguration : IEntityTypeConfiguration<UserCredentialEntity>
{
    public void Configure(EntityTypeBuilder<UserCredentialEntity> builder)
    {
        builder.ToTable("UserCredentials");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.UserId).IsRequired();
        builder.Property(e => e.CredentialId).IsRequired();
        builder.HasIndex(e => e.CredentialId).IsUnique();
        builder.Property(e => e.PublicKey).IsRequired();
        builder.Property(e => e.SignCount).IsRequired();

        builder.HasOne(e => e.User)
            .WithMany(u => u.Credentials)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}