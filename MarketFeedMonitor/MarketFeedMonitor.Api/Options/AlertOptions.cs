namespace MarketFeedMonitor.Api.Options;

public sealed class AlertOptions
{
    public const string SectionName = "Alerts";

    public int StaleFeedThresholdSeconds { get; set; }
    public int ConsecutiveFailureThreshold { get; set; }
}
