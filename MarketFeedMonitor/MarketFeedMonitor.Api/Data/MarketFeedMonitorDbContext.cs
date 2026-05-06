using MarketFeedMonitor.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace MarketFeedMonitor.Api.Data;

public sealed class MarketFeedMonitorDbContext(DbContextOptions<MarketFeedMonitorDbContext> options) : DbContext(options)
{
    public DbSet<InstrumentDefinition> Instruments => Set<InstrumentDefinition>();
    public DbSet<MarketSnapshot> Snapshots => Set<MarketSnapshot>();
    public DbSet<FeedStatus> FeedStatuses => Set<FeedStatus>();
    public DbSet<AlertRecord> Alerts => Set<AlertRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AlertRecord>(builder =>
        {
            builder.ToTable("alert_records");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.AlertType)
                .HasConversion<string>()
                .HasMaxLength(32);

            builder.Property(x => x.Severity)
                .HasConversion<string>()
                .HasMaxLength(32);

            builder.Property(x => x.Message)
                .IsRequired()
                .HasMaxLength(512);

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.HasIndex(x => new { x.InstrumentId, x.IsActive });
            builder.HasIndex(x => x.CreatedAt);

            builder.HasOne(x => x.Instrument)
                .WithMany(x => x.Alerts)
                .HasForeignKey(x => x.InstrumentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<FeedStatus>(builder =>
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
        });

        modelBuilder.Entity<InstrumentDefinition>(builder =>
        {
            builder.ToTable("instrument_definitions");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Symbol)
                .IsRequired()
                .HasMaxLength(32);

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(128);

            builder.Property(x => x.AssetType)
                .HasConversion<string>()
                .HasMaxLength(32);

            builder.Property(x => x.PrimarySource)
                .HasConversion<string>()
                .HasMaxLength(32);

            builder.HasIndex(x => x.Symbol)
                .IsUnique();
        });

        modelBuilder.Entity<MarketSnapshot>(builder =>
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
        });
    }
}
