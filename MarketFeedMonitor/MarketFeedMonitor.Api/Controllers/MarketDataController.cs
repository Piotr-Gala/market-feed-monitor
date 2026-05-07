using MarketFeedMonitor.Api.Data;
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
}
