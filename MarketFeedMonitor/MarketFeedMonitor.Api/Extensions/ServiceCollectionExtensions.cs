using MarketFeedMonitor.Api.Background;
using MarketFeedMonitor.Api.Configuration;

namespace MarketFeedMonitor.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApiServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<BinanceOptions>(configuration.GetSection(BinanceOptions.SectionName));
        services.Configure<TwelveDataOptions>(configuration.GetSection(TwelveDataOptions.SectionName));
        services.Configure<AlertOptions>(configuration.GetSection(AlertOptions.SectionName));

        services.AddHostedService<BinancePollingHostedService>();
        services.AddHostedService<TwelveDataPollingHostedService>();

        return services;
    }
}
