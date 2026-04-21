using MarketFeedMonitor.Core.Snapshots;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MarketFeedMonitor.Infrastructure.Persistence.Configurations;

public sealed class MarketSnapshotConfiguration : IEntityTypeConfiguration<MarketSnapshot>
{
    public void Configure(EntityTypeBuilder<MarketSnapshot> builder)
    {
        builder.ToTable("market_snapshots");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Price)
            .HasPrecision(18, 8);

        builder.Property(x => x.Change1hPercent)
            .HasPrecision(8, 4);

        builder.Property(x => x.Source)
            .HasConversion<string>()
            .HasMaxLength(32);

        builder.Property(x => x.SourceTimestamp)
            .IsRequired();

        builder.Property(x => x.ReceivedAt)
            .IsRequired();

        builder.HasIndex(x => new { x.InstrumentId, x.ReceivedAt });
        builder.HasIndex(x => new { x.Source, x.SourceTimestamp });

        builder.HasOne(x => x.Instrument)
            .WithMany(x => x.Snapshots)
            .HasForeignKey(x => x.InstrumentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
