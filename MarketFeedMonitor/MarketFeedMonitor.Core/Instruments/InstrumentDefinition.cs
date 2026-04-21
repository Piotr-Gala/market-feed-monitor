using MarketFeedMonitor.Core.Alerts;
using MarketFeedMonitor.Core.Feeds;
using MarketFeedMonitor.Core.Snapshots;

namespace MarketFeedMonitor.Core.Instruments;

public sealed class InstrumentDefinition
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Symbol { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public AssetType AssetType { get; set; }
    public DataSourceType PrimarySource { get; set; }
    public bool IsTracked { get; set; } = true;

    public List<MarketSnapshot> Snapshots { get; set; } = [];
    public List<AlertRecord> Alerts { get; set; } = [];
}
