using MarketFeedMonitor.Api.Data;
using MarketFeedMonitor.Api.Models;
using MarketFeedMonitor.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MarketFeedMonitor.Api.Controllers;

[ApiController]
[Route("api/market-data")]
public sealed class MarketDataController(
    MarketFeedMonitorDbContext dbContext,
    MarketDataIngestionService marketDataIngestionService,
    ILogger<MarketDataController> logger) : ControllerBase
{
    [HttpPost("binance/fetch")]
    public async Task<IActionResult> FetchBinanceSnapshots(CancellationToken cancellationToken)
    {
        try
        {
            var result = await marketDataIngestionService.FetchBinanceSnapshotsAsync(cancellationToken);

            return Ok(new
            {
                source = result.Source,
                savedSnapshots = result.SavedSnapshots,
                createdInstruments = result.CreatedInstruments,
                executedAt = result.ExecutedAt
            });
        }
        catch (HttpRequestException exception)
        {
            logger.LogError(exception, "Manual Binance fetch failed because the external API call failed.");

            return Problem(
                title: "Binance fetch failed.",
                detail: exception.Message,
                statusCode: StatusCodes.Status502BadGateway);
        }
        catch (InvalidOperationException exception)
        {
            logger.LogError(exception, "Manual Binance fetch failed because the ingestion flow is invalid.");

            return Problem(
                title: "Binance fetch failed.",
                detail: exception.Message,
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    [HttpGet("feed-statuses")]
    public async Task<IActionResult> GetFeedStatuses(CancellationToken cancellationToken)
    {
        var feedStatuses = await dbContext.FeedStatuses
            .OrderBy(feedStatus => feedStatus.Source)
            .Select(feedStatus => new
            {
                source = feedStatus.Source.ToString(),
                isActive = feedStatus.IsActive,
                lastAttemptAt = feedStatus.LastAttemptAt,
                lastSuccessfulFetchAt = feedStatus.LastSuccessfulFetchAt,
                consecutiveFailures = feedStatus.ConsecutiveFailures,
                status = feedStatus.Status.ToString()
            })
            .ToListAsync(cancellationToken);

        return Ok(feedStatuses);
    }

    [HttpGet("latest-snapshots")]
    public async Task<IActionResult> GetLatestSnapshots(CancellationToken cancellationToken)
    {
        var latestSnapshotTimes = dbContext.Snapshots
            .GroupBy(snapshot => snapshot.InstrumentId)
            .Select(group => new
            {
                InstrumentId = group.Key,
                ReceivedAt = group.Max(snapshot => snapshot.ReceivedAt)
            });

        var latestSnapshots = await dbContext.Snapshots
            .Include(snapshot => snapshot.Instrument)
            .Join(
                latestSnapshotTimes,
                snapshot => new { snapshot.InstrumentId, snapshot.ReceivedAt },
                latest => new { latest.InstrumentId, latest.ReceivedAt },
                (snapshot, latest) => snapshot)
            .OrderBy(snapshot => snapshot.Instrument.Symbol)
            .ToListAsync(cancellationToken);

        var response = new List<object>();

        foreach (var snapshot in latestSnapshots)
        {
            var oneHourAgo = snapshot.ReceivedAt.AddHours(-1);

            var comparisonSnapshot = await dbContext.Snapshots
                .Where(candidate =>
                    candidate.InstrumentId == snapshot.InstrumentId &&
                    candidate.ReceivedAt <= oneHourAgo)
                .OrderByDescending(candidate => candidate.ReceivedAt)
                .FirstOrDefaultAsync(cancellationToken);

            var change1hPercent = comparisonSnapshot is null || comparisonSnapshot.Price == 0
                ? (decimal?)null
                : ((snapshot.Price - comparisonSnapshot.Price) / comparisonSnapshot.Price) * 100;

            response.Add(new
            {
                symbol = snapshot.Instrument.Symbol,
                name = snapshot.Instrument.Name,
                assetType = snapshot.Instrument.AssetType.ToString(),
                source = snapshot.Source.ToString(),
                price = snapshot.Price,
                change1hPercent,
                sourceTimestamp = snapshot.SourceTimestamp,
                receivedAt = snapshot.ReceivedAt
            });
        }

        return Ok(response);
    }


    [HttpGet("feed-summary")]
    public async Task<IActionResult> GetFeedSummary(CancellationToken cancellationToken)
    {
        var trackedInstruments = await dbContext.Instruments
            .CountAsync(instrument => instrument.IsTracked, cancellationToken);

        var storedSnapshots = await dbContext.Snapshots
            .CountAsync(cancellationToken);

        var activeAlerts = await dbContext.Alerts
            .CountAsync(alert => alert.IsActive, cancellationToken);

        var totalFeeds = await dbContext.FeedStatuses
            .CountAsync(cancellationToken);

        var healthyFeeds = await dbContext.FeedStatuses
            .CountAsync(feedStatus => feedStatus.Status == FeedState.Healthy, cancellationToken);

        var staleFeeds = await dbContext.FeedStatuses
            .CountAsync(feedStatus => feedStatus.Status == FeedState.Stale, cancellationToken);

        var downFeeds = await dbContext.FeedStatuses
            .CountAsync(feedStatus => feedStatus.Status == FeedState.Down, cancellationToken);

        var lastSnapshotReceivedAt = await dbContext.Snapshots
            .Select(snapshot => (DateTimeOffset?)snapshot.ReceivedAt)
            .MaxAsync(cancellationToken);

        var lastSuccessfulFetchAt = await dbContext.FeedStatuses
            .Select(feedStatus => feedStatus.LastSuccessfulFetchAt)
            .MaxAsync(cancellationToken);

        return Ok(new
        {
            trackedInstruments,
            storedSnapshots,
            activeAlerts,
            feeds = new
            {
                total = totalFeeds,
                healthy = healthyFeeds,
                stale = staleFeeds,
                down = downFeeds
            },
            lastSnapshotReceivedAt,
            lastSuccessfulFetchAt
        });
    }

    [HttpGet("active-alerts")]
    public async Task<IActionResult> GetActiveAlerts(CancellationToken cancellationToken)
    {
        var activeAlerts = await dbContext.Alerts
            .Where(alert => alert.IsActive)
            .OrderByDescending(alert => alert.CreatedAt)
            .Select(alert => new
            {
                id = alert.Id,
                symbol = alert.Instrument.Symbol,
                alertType = alert.AlertType.ToString(),
                severity = alert.Severity.ToString(),
                message = alert.Message,
                createdAt = alert.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return Ok(activeAlerts);
    }
}
