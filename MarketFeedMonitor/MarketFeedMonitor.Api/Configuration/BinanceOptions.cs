namespace MarketFeedMonitor.Api.Configuration;

public sealed class BinanceOptions
{
    public const string SectionName = "MarketData:Sources:Binance";

    public string BaseUrl { get; set; } = string.Empty;
    public int PollIntervalSeconds { get; set; }
    public List<string> Instruments { get; set; } = [];
}
