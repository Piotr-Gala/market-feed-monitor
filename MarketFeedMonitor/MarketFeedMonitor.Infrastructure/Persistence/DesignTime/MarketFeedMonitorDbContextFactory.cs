using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MarketFeedMonitor.Infrastructure.Persistence.DesignTime;

public sealed class MarketFeedMonitorDbContextFactory : IDesignTimeDbContextFactory<MarketFeedMonitorDbContext>
{
    public MarketFeedMonitorDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<MarketFeedMonitorDbContext>();

        optionsBuilder.UseNpgsql(
            "Host=localhost;Port=5432;Database=market_feed_monitor;Username=postgres;Password=postgres");

        return new MarketFeedMonitorDbContext(optionsBuilder.Options);
    }
}
