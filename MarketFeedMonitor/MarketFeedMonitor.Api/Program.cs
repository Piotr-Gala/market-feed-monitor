using MarketFeedMonitor.Api.Background;
using MarketFeedMonitor.Api.Data;
using MarketFeedMonitor.Api.External;
using MarketFeedMonitor.Api.Services;
using MarketFeedMonitor.Api.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.Configure<BinanceOptions>(
    builder.Configuration.GetSection(BinanceOptions.SectionName));
builder.Services.Configure<TwelveDataOptions>(
    builder.Configuration.GetSection(TwelveDataOptions.SectionName));
builder.Services.Configure<AlertOptions>(
    builder.Configuration.GetSection(AlertOptions.SectionName));

var connectionString = builder.Configuration.GetConnectionString("Postgres")
    ?? throw new InvalidOperationException("Connection string 'Postgres' is missing.");

builder.Services.AddDbContext<MarketFeedMonitorDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddHttpClient<BinanceClient>((serviceProvider, httpClient) =>
{
    var options = serviceProvider.GetRequiredService<IOptions<BinanceOptions>>().Value;

    if (string.IsNullOrWhiteSpace(options.BaseUrl))
    {
        throw new InvalidOperationException("Binance base URL is missing.");
    }

    httpClient.BaseAddress = new Uri(options.BaseUrl, UriKind.Absolute);
});

builder.Services.AddScoped<MarketDataIngestionService>();
builder.Services.AddScoped<AlertService>();
builder.Services.AddHostedService<BinancePollingHostedService>();
builder.Services.AddHostedService<TwelveDataPollingHostedService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
