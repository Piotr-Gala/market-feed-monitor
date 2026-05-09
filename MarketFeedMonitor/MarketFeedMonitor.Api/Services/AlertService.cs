using MarketFeedMonitor.Api.Data;
using MarketFeedMonitor.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace MarketFeedMonitor.Api.Services;

public sealed class AlertService(MarketFeedMonitorDbContext dbContext)
{
    public async Task SyncFeedAlertsAsync(
        DataSourceType source,
        FeedState feedState,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var instruments = await dbContext.Instruments
            .Where(instrument => instrument.PrimarySource == source && instrument.IsTracked)
            .ToListAsync(cancellationToken);

        if (instruments.Count == 0)
        {
            return;
        }

        if (feedState == FeedState.Healthy)
        {
            await ResolveActiveFeedAlertsAsync(instruments, cancellationToken);
            return;
        }

        var targetAlertType = feedState == FeedState.Down
            ? AlertType.FeedDown
            : AlertType.StaleFeed;

        var targetSeverity = feedState == FeedState.Down
            ? AlertSeverity.Critical
            : AlertSeverity.Warning;

        await ResolveOppositeFeedAlertsAsync(instruments, targetAlertType, cancellationToken);
        await CreateMissingFeedAlertsAsync(
            instruments,
            source,
            targetAlertType,
            targetSeverity,
            now,
            cancellationToken);
    }

    private async Task ResolveActiveFeedAlertsAsync(
        IReadOnlyCollection<InstrumentDefinition> instruments,
        CancellationToken cancellationToken)
    {
        var instrumentIds = instruments.Select(instrument => instrument.Id).ToArray();

        var activeAlerts = await dbContext.Alerts
            .Where(alert =>
                instrumentIds.Contains(alert.InstrumentId) &&
                alert.IsActive &&
                (alert.AlertType == AlertType.StaleFeed || alert.AlertType == AlertType.FeedDown))
            .ToListAsync(cancellationToken);

        foreach (var alert in activeAlerts)
        {
            alert.IsActive = false;
        }
    }

    private async Task ResolveOppositeFeedAlertsAsync(
        IReadOnlyCollection<InstrumentDefinition> instruments,
        AlertType targetAlertType,
        CancellationToken cancellationToken)
    {
        var instrumentIds = instruments.Select(instrument => instrument.Id).ToArray();

        var oppositeAlertType = targetAlertType == AlertType.FeedDown
            ? AlertType.StaleFeed
            : AlertType.FeedDown;

        var activeAlerts = await dbContext.Alerts
            .Where(alert =>
                instrumentIds.Contains(alert.InstrumentId) &&
                alert.IsActive &&
                alert.AlertType == oppositeAlertType)
            .ToListAsync(cancellationToken);

        foreach (var alert in activeAlerts)
        {
            alert.IsActive = false;
        }
    }

    private async Task CreateMissingFeedAlertsAsync(
        IReadOnlyCollection<InstrumentDefinition> instruments,
        DataSourceType source,
        AlertType alertType,
        AlertSeverity severity,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var instrumentIds = instruments.Select(instrument => instrument.Id).ToArray();

        var existingActiveAlertInstrumentIds = await dbContext.Alerts
            .Where(alert =>
                instrumentIds.Contains(alert.InstrumentId) &&
                alert.IsActive &&
                alert.AlertType == alertType)
            .Select(alert => alert.InstrumentId)
            .ToListAsync(cancellationToken);

        var existingIds = existingActiveAlertInstrumentIds.ToHashSet();

        foreach (var instrument in instruments.Where(instrument => !existingIds.Contains(instrument.Id)))
        {
            dbContext.Alerts.Add(new AlertRecord
            {
                InstrumentId = instrument.Id,
                AlertType = alertType,
                Severity = severity,
                Message = $"{source} feed is {alertType} for {instrument.Symbol}.",
                CreatedAt = now,
                IsActive = true
            });
        }
    }
}

