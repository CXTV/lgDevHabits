using lgDevHabit.Api.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace lgDevHabit.Api.Database.Configurations;


public sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.UserId)
            .HasMaxLength(300);

        builder.Property(e => e.Token)
            .HasMaxLength(1000);

        builder.HasIndex(e => e.Token)
            .IsUnique();

        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
