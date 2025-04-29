using lgDevHabit.Api.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace lgDevHabit.Api.Database.Configurations;

public sealed class TagConfiguration : IEntityTypeConfiguration<Tag>
{
    public void Configure(EntityTypeBuilder<Tag> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.UserId).HasMaxLength(500);

        builder.Property(t => t.Id).HasMaxLength(500);

        builder.Property(t => t.Name).IsRequired().HasMaxLength(50);

        builder.Property(t => t.Description).HasMaxLength(500);

        builder.HasIndex(t => new { t.Name }).IsUnique();


        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(t => t.UserId);
    }
}
