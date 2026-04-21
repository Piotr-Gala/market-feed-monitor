using MarketFeedMonitor.Api.Configuration;
using Microsoft.Extensions.Options;

namespace MarketFeedMonitor.Api.Background;

public sealed class TwelveDataPollingHostedService(
    IOptionsMonitor<TwelveDataOptions> optionsMonitor,
    ILogger<TwelveDataPollingHostedService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var options = optionsMonitor.CurrentValue;
        var interval = TimeSpan.FromMinutes(Math.Max(options.PollIntervalMinutes, 1));

        logger.LogInformation(
            "Twelve Data polling skeleton started. Interval: {IntervalMinutes}m. Instruments: {Instruments}",
            interval.TotalMinutes,
            string.Join(", ", options.Instruments));

        using var timer = new PeriodicTimer(interval);

        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                logger.LogDebug("Twelve Data polling skeleton tick.");
            }
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("Twelve Data polling skeleton stopped.");
        }
    }
}
