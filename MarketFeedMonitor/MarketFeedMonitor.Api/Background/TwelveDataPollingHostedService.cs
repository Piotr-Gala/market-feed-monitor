using MarketFeedMonitor.Api.Options;
using MarketFeedMonitor.Api.Services;
using Microsoft.Extensions.Options;

namespace MarketFeedMonitor.Api.Background;

public sealed class TwelveDataPollingHostedService(
    IServiceScopeFactory scopeFactory,
    IOptionsMonitor<TwelveDataOptions> optionsMonitor,
    ILogger<TwelveDataPollingHostedService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var options = optionsMonitor.CurrentValue;
        var interval = TimeSpan.FromMinutes(Math.Max(options.PollIntervalMinutes, 1));

        logger.LogInformation(
            "Twelve Data polling started. Interval: {IntervalMinutes}m. Instruments: {Instruments}",
            interval.TotalMinutes,
            string.Join(", ", options.Instruments));

        await FetchSnapshotsAsync(stoppingToken);

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
            logger.LogInformation("Twelve Data polling stopped.");
        }
    }

    private async Task FetchSnapshotsAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var ingestionService = scope.ServiceProvider.GetRequiredService<MarketDataIngestionService>();

        try
        {
            var result = await ingestionService.FetchTwelveDataSnapshotsAsync(cancellationToken);

            logger.LogInformation(
                "Twelve Data polling saved {SnapshotCount} snapshots.",
                result.SavedSnapshots);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Twelve Data polling failed.");
        }
    }
}
