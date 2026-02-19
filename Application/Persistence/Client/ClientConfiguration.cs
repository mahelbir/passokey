using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Application.Persistence.Client;

public class ClientConfiguration : IEntityTypeConfiguration<ClientEntity>
{
    public void Configure(EntityTypeBuilder<ClientEntity> builder)
    {
        builder.ToTable("Clients");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Name).IsRequired().HasMaxLength(255);
        builder.Property(e => e.SecretKey).IsRequired().HasMaxLength(64);
        builder.HasIndex(e => e.SecretKey).IsUnique();
        builder.Property(e => e.IsRegistrationEnabled).HasDefaultValue(true);
        builder.Property(e => e.IsAdmin).HasDefaultValue(false);
        builder.Property(e => e.RedirectUriList).HasConversion(
            v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
            v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>(),
            new ValueComparer<List<string>>(
                (a, b) => a != null && b != null && a.SequenceEqual(b),
                c => c.Aggregate(0, (hash, item) => HashCode.Combine(hash, item.GetHashCode())),
                c => c.ToList()
            )
        );

        builder.HasMany(e => e.UserPermissions)
            .WithOne(e => e.Client)
            .HasForeignKey(e => e.ClientId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}