using MarketFeedMonitor.Api.Data;
using MarketFeedMonitor.Api.External;
using MarketFeedMonitor.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace MarketFeedMonitor.Api.Services;

public sealed class MarketDataIngestionService(
    BinanceClient binanceClient,
    MarketFeedMonitorDbContext dbContext,
    IOptionsMonitor<BinanceOptions> optionsMonitor,
    ILogger<MarketDataIngestionService> logger)
{
    public async Task<MarketDataIngestionResult> FetchBinanceSnapshotsAsync(
        CancellationToken cancellationToken = default)
    {
        var configuredSymbols = optionsMonitor.CurrentValue.Instruments
            .Where(symbol => !string.IsNullOrWhiteSpace(symbol))
            .Select(symbol => symbol.Trim().ToUpperInvariant())
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        if (configuredSymbols.Length == 0)
        {
            throw new InvalidOperationException("No Binance instruments are configured.");
        }

        logger.LogInformation(
            "Starting manual Binance ingestion for {InstrumentCount} instruments.",
            configuredSymbols.Length);

        IReadOnlyCollection<BinancePriceQuote> quotes;
        var now = DateTimeOffset.UtcNow;

        try
        {
            quotes = await binanceClient.GetLatestPricesAsync(configuredSymbols, cancellationToken);
        }
        catch
        {
            await MarkFeedFailureAsync(DataSourceType.Binance, now, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
            throw;
        }

        var existingInstruments = (await dbContext.Instruments
                .Where(instrument => configuredSymbols.Contains(instrument.Symbol))
                .ToListAsync(cancellationToken))
            .ToDictionary(
                instrument => instrument.Symbol.ToUpperInvariant(),
                StringComparer.Ordinal);

        var createdInstruments = new List<string>();

        foreach (var symbol in configuredSymbols)
        {
            if (existingInstruments.ContainsKey(symbol))
            {
                continue;
            }

            var instrument = new InstrumentDefinition
            {
                Symbol = symbol,
                Name = symbol,
                AssetType = AssetType.Crypto,
                PrimarySource = DataSourceType.Binance,
                IsTracked = true
            };

            dbContext.Instruments.Add(instrument);
            existingInstruments[symbol] = instrument;
            createdInstruments.Add(symbol);
        }

        var snapshots = quotes.Select(quote =>
        {
            if (!existingInstruments.TryGetValue(quote.Symbol, out var instrument))
            {
                throw new InvalidOperationException(
                    $"Instrument '{quote.Symbol}' is missing in the database context.");
            }

            return new MarketSnapshot
            {
                InstrumentId = instrument.Id,
                Price = quote.Price,
                Change1hPercent = null,
                Source = DataSourceType.Binance,
                SourceTimestamp = now,
                ReceivedAt = now
            };
        }).ToArray();

        dbContext.Snapshots.AddRange(snapshots);
        await MarkFeedSuccessAsync(DataSourceType.Binance, now, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Saved {SnapshotCount} Binance snapshots. Created {CreatedInstrumentCount} instruments.",
            snapshots.Length,
            createdInstruments.Count);

        return new MarketDataIngestionResult(
            DataSourceType.Binance.ToString(),
            snapshots.Length,
            createdInstruments,
            now);
    }

    private async Task MarkFeedSuccessAsync(
        DataSourceType source,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var feedStatus = await dbContext.FeedStatuses
            .SingleOrDefaultAsync(status => status.Source == source, cancellationToken);

        if (feedStatus is null)
        {
            feedStatus = new FeedStatus
            {
                Source = source
            };

            dbContext.FeedStatuses.Add(feedStatus);
        }

        feedStatus.IsActive = true;
        feedStatus.LastAttemptAt = now;
        feedStatus.LastSuccessfulFetchAt = now;
        feedStatus.ConsecutiveFailures = 0;
        feedStatus.Status = FeedState.Healthy;
    }

    private async Task MarkFeedFailureAsync(
        DataSourceType source,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var feedStatus = await dbContext.FeedStatuses
            .SingleOrDefaultAsync(status => status.Source == source, cancellationToken);

        if (feedStatus is null)
        {
            feedStatus = new FeedStatus
            {
                Source = source
            };

            dbContext.FeedStatuses.Add(feedStatus);
        }

        feedStatus.IsActive = true;
        feedStatus.LastAttemptAt = now;
        feedStatus.ConsecutiveFailures += 1;
        feedStatus.Status = FeedState.Down;
    }

}

public sealed record MarketDataIngestionResult(
    string Source,
    int SavedSnapshots,
    IReadOnlyCollection<string> CreatedInstruments,
    DateTimeOffset ExecutedAt);
