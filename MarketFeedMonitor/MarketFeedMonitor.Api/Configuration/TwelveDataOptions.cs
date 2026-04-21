namespace MarketFeedMonitor.Api.Configuration;

public sealed class TwelveDataOptions
{
    public const string SectionName = "MarketData:Sources:TwelveData";

    public string BaseUrl { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public int PollIntervalMinutes { get; set; }
    public List<string> Instruments { get; set; } = [];
}
