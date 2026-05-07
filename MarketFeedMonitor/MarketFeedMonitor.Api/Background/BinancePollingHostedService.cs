using MarketFeedMonitor.Api.External;
using MarketFeedMonitor.Api.Services;
using Microsoft.Extensions.Options;

namespace MarketFeedMonitor.Api.Background;

public sealed class BinancePollingHostedService(
    IServiceScopeFactory serviceScopeFactory,
    IOptionsMonitor<BinanceOptions> optionsMonitor,
    ILogger<BinancePollingHostedService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var options = optionsMonitor.CurrentValue;
        var interval = TimeSpan.FromSeconds(Math.Max(options.PollIntervalSeconds, 1));

        logger.LogInformation(
            "Binance polling started. Interval: {IntervalSeconds}s. Instruments: {Instruments}",
            interval.TotalSeconds,
            string.Join(", ", options.Instruments));

        using var timer = new PeriodicTimer(interval);

        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await FetchSnapshotsAsync(stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("Binance polling stopped.");
        }
    }

    private async Task FetchSnapshotsAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = serviceScopeFactory.CreateScope();
            var ingestionService = scope.ServiceProvider.GetRequiredService<MarketDataIngestionService>();

            var result = await ingestionService.FetchBinanceSnapshotsAsync(cancellationToken);

            logger.LogInformation(
                "Binance polling saved {SnapshotCount} snapshots.",
                result.SavedSnapshots);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Binance polling failed.");
        }
    }
}
