using MarketFeedMonitor.Api.Configuration;
using Microsoft.Extensions.Options;

namespace MarketFeedMonitor.Api.Background;

public sealed class BinancePollingHostedService(
    IOptionsMonitor<BinanceOptions> optionsMonitor,
    ILogger<BinancePollingHostedService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var options = optionsMonitor.CurrentValue;
        var interval = TimeSpan.FromSeconds(Math.Max(options.PollIntervalSeconds, 1));

        logger.LogInformation(
            "Binance polling skeleton started. Interval: {IntervalSeconds}s. Instruments: {Instruments}",
            interval.TotalSeconds,
            string.Join(", ", options.Instruments));

        using var timer = new PeriodicTimer(interval);

        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                logger.LogDebug("Binance polling skeleton tick.");
            }
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("Binance polling skeleton stopped.");
        }
    }
}
