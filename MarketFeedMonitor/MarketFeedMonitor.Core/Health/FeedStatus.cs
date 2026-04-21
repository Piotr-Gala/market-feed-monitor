using MarketFeedMonitor.Core.Feeds;

namespace MarketFeedMonitor.Core.Health;

public sealed class FeedStatus
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DataSourceType Source { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset? LastAttemptAt { get; set; }
    public DateTimeOffset? LastSuccessfulFetchAt { get; set; }
    public int ConsecutiveFailures { get; set; }
    public FeedState Status { get; set; } = FeedState.Down;
}
