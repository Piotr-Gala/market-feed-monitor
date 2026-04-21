using MarketFeedMonitor.Core.Alerts;
using MarketFeedMonitor.Core.Health;
using MarketFeedMonitor.Core.Instruments;
using MarketFeedMonitor.Core.Snapshots;
using Microsoft.EntityFrameworkCore;

namespace MarketFeedMonitor.Infrastructure.Persistence;

public sealed class MarketFeedMonitorDbContext(DbContextOptions<MarketFeedMonitorDbContext> options) : DbContext(options)
{
    public DbSet<InstrumentDefinition> Instruments => Set<InstrumentDefinition>();
    public DbSet<MarketSnapshot> Snapshots => Set<MarketSnapshot>();
    public DbSet<FeedStatus> FeedStatuses => Set<FeedStatus>();
    public DbSet<AlertRecord> Alerts => Set<AlertRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MarketFeedMonitorDbContext).Assembly);
    }
}
