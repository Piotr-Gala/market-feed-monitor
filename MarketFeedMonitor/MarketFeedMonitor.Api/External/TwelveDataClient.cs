using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using MarketFeedMonitor.Api.Options;
using Microsoft.Extensions.Options;

namespace MarketFeedMonitor.Api.External;

public sealed class TwelveDataClient(
    HttpClient httpClient,
    IOptionsMonitor<TwelveDataOptions> optionsMonitor)
{
    public async Task<IReadOnlyCollection<TwelveDataPriceQuote>> GetLatestPricesAsync(
        IEnumerable<string> symbols,
        CancellationToken cancellationToken = default)
    {
        var apiKey = optionsMonitor.CurrentValue.ApiKey;

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException("Twelve Data API key is missing.");
        }

        var normalizedSymbols = symbols
            .Where(symbol => !string.IsNullOrWhiteSpace(symbol))
            .Select(symbol => symbol.Trim().ToUpperInvariant())
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        var quotes = new List<TwelveDataPriceQuote>(normalizedSymbols.Length);

        foreach (var symbol in normalizedSymbols)
        {
            var response = await httpClient.GetFromJsonAsync<TwelveDataPriceResponse>(
                $"/price?symbol={Uri.EscapeDataString(symbol)}&apikey={Uri.EscapeDataString(apiKey)}",
                cancellationToken);

            if (response is null || string.IsNullOrWhiteSpace(response.Price))
            {
                throw new InvalidOperationException(
                    $"Twelve Data returned an empty price payload for symbol '{symbol}'.");
            }

            if (!decimal.TryParse(
                response.Price,
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out var price))
            {
                throw new InvalidOperationException(
                    $"Twelve Data returned an invalid price '{response.Price}' for symbol '{symbol}'.");
            }

            quotes.Add(new TwelveDataPriceQuote(symbol, price));
        }

        return quotes;
    }

    private sealed class TwelveDataPriceResponse
    {
        [JsonPropertyName("price")]
        public string Price { get; init; } = string.Empty;
    }
}

public sealed record TwelveDataPriceQuote(string Symbol, decimal Price);
