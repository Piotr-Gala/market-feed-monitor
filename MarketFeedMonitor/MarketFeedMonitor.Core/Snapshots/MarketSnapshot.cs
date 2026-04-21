using MarketFeedMonitor.Core.Feeds;
using MarketFeedMonitor.Core.Instruments;

namespace MarketFeedMonitor.Core.Snapshots;

public sealed class MarketSnapshot
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid InstrumentId { get; set; }
    public decimal Price { get; set; }
    public decimal? Change1hPercent { get; set; }
    public DataSourceType Source { get; set; }
    public DateTimeOffset SourceTimestamp { get; set; }
    public DateTimeOffset ReceivedAt { get; set; } = DateTimeOffset.UtcNow;

    public InstrumentDefinition Instrument { get; set; } = null!;
}
