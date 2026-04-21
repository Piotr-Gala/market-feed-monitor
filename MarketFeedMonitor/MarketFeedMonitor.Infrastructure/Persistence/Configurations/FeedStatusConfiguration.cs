using MarketFeedMonitor.Core.Health;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MarketFeedMonitor.Infrastructure.Persistence.Configurations;

public sealed class FeedStatusConfiguration : IEntityTypeConfiguration<FeedStatus>
{
    public void Configure(EntityTypeBuilder<FeedStatus> builder)
    {
        builder.ToTable("feed_statuses");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Source)
            .HasConversion<string>()
            .HasMaxLength(32);

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(32);

        builder.HasIndex(x => x.Source)
            .IsUnique();
    }
}
