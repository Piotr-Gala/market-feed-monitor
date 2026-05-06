using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace MarketFeedMonitor.Api.External;

public sealed class BinanceClient(HttpClient httpClient)
{
    public async Task<IReadOnlyCollection<BinancePriceQuote>> GetLatestPricesAsync(
        IEnumerable<string> symbols,
        CancellationToken cancellationToken = default)
    {
        var normalizedSymbols = symbols
            .Where(symbol => !string.IsNullOrWhiteSpace(symbol))
            .Select(symbol => symbol.Trim().ToUpperInvariant())
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        if (normalizedSymbols.Length == 0)
        {
            return [];
        }

        var quotes = new List<BinancePriceQuote>(normalizedSymbols.Length);

        foreach (var symbol in normalizedSymbols)
        {
            var response = await httpClient.GetFromJsonAsync<BinanceTickerPriceResponse>(
                $"/api/v3/ticker/price?symbol={Uri.EscapeDataString(symbol)}",
                cancellationToken);

            if (response is null ||
                string.IsNullOrWhiteSpace(response.Symbol) ||
                string.IsNullOrWhiteSpace(response.Price))
            {
                throw new InvalidOperationException(
                    $"Binance returned an empty price payload for symbol '{symbol}'.");
            }

            if (!decimal.TryParse(
                response.Price,
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out var price))
            {
                throw new InvalidOperationException(
                    $"Binance returned an invalid price '{response.Price}' for symbol '{symbol}'.");
            }

            quotes.Add(new BinancePriceQuote(response.Symbol.Trim().ToUpperInvariant(), price));
        }

        return quotes;
    }

    private sealed class BinanceTickerPriceResponse
    {
        [JsonPropertyName("symbol")]
        public string Symbol { get; init; } = string.Empty;

        [JsonPropertyName("price")]
        public string Price { get; init; } = string.Empty;
    }
}

public sealed record BinancePriceQuote(string Symbol, decimal Price);
