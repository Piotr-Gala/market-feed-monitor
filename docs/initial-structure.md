# Initial Project Structure

This document defines the first technical shape of the project. It is intentionally small and biased toward MVP delivery.

## Confirmed MVP Inputs

- Source 1: Binance, every 15 seconds
- Source 2: Twelve Data, every 5 minutes
- Instruments: BTCUSDT, ETHUSDT, EURUSD, XAUUSD
- Backend: ASP.NET Core Web API
- Frontend: React + TypeScript + Vite
- Database: PostgreSQL
- ORM: Entity Framework Core
- Python: later only if the core application is already working

## Important Scope Rule

Do not split the backend into many projects too early.

For this MVP, a small and practical structure is enough:

- one API host project
- one core project for domain models and rules
- one infrastructure project for database and external APIs
- one frontend app

There is no need for a separate worker service, messaging layer, or a dedicated application layer yet.

## Suggested Solution Layout

```text
/
  README.md
  docs/
    initial-structure.md
  src/
    MarketFeedMonitor.Api/
    MarketFeedMonitor.Core/
    MarketFeedMonitor.Infrastructure/
    market-feed-monitor-web/
  tests/
    MarketFeedMonitor.Api.Tests/   (optional later)
```

## Suggested Project Names

- Solution: `MarketFeedMonitor.sln`
- Backend host: `MarketFeedMonitor.Api`
- Core models and rules: `MarketFeedMonitor.Core`
- Database and integrations: `MarketFeedMonitor.Infrastructure`
- Frontend app: `market-feed-monitor-web`
- Test project later if needed: `MarketFeedMonitor.Api.Tests`

## Suggested Namespaces

- `MarketFeedMonitor.Api`
- `MarketFeedMonitor.Api.Controllers`
- `MarketFeedMonitor.Api.Background`
- `MarketFeedMonitor.Api.Configuration`
- `MarketFeedMonitor.Core`
- `MarketFeedMonitor.Core.Instruments`
- `MarketFeedMonitor.Core.Feeds`
- `MarketFeedMonitor.Core.Snapshots`
- `MarketFeedMonitor.Core.Health`
- `MarketFeedMonitor.Core.Alerts`
- `MarketFeedMonitor.Infrastructure`
- `MarketFeedMonitor.Infrastructure.Persistence`
- `MarketFeedMonitor.Infrastructure.External.Binance`
- `MarketFeedMonitor.Infrastructure.External.TwelveData`

## Suggested Module Ownership

Keep module ownership obvious:

- `MarketFeedMonitor.Api`
  Responsibility: HTTP endpoints, hosted services, configuration binding, dependency injection
- `MarketFeedMonitor.Core`
  Responsibility: domain models, health rules, alert rules, instrument definitions
- `MarketFeedMonitor.Infrastructure`
  Responsibility: PostgreSQL access, EF Core mappings, external API clients, source-specific DTO mapping
- `market-feed-monitor-web`
  Responsibility: dashboard UI, API calls, view models, status rendering

## Suggested Backend Modules

Keep the module list short:

- `Feeds`
  Responsibility: source definitions, polling setup, and feed metadata
- `Snapshots`
  Responsibility: raw market snapshots and snapshot storage
- `Health`
  Responsibility: freshness checks, failure tracking, and feed status
- `Alerts`
  Responsibility: simple alert rules and active alert state
- `Instruments`
  Responsibility: canonical instrument identifiers used by the system

Suggested starter classes inside those modules:

- `InstrumentDefinition`
- `MarketSnapshot`
- `FeedStatus`
- `AlertRecord`
- `BinanceClient`
- `TwelveDataClient`
- `BinancePollerHostedService`
- `TwelveDataPollerHostedService`
- `MarketFeedMonitorDbContext`

## Suggested Frontend Structure

```text
src/
  app/
  api/
  components/
  features/
    dashboard/
    alerts/
    feeds/
  types/
```

This is enough for the first version. There is no need for a large frontend architecture yet.

## Suggested Backend Folder Shape

### `MarketFeedMonitor.Api`

```text
Controllers/
Background/
Configuration/
Extensions/
Program.cs
appsettings.json
appsettings.Development.json
```

Notes:

- `Background/` holds polling jobs implemented as hosted services
- `Configuration/` holds options classes bound from configuration
- `Extensions/` holds service registration and startup helpers

### `MarketFeedMonitor.Core`

```text
Entities/
Enums/
Interfaces/
ValueObjects/
```

Notes:

- keep only the types that are actually needed
- do not add abstractions just to look clean

### `MarketFeedMonitor.Infrastructure`

```text
Persistence/
  MarketFeedMonitorDbContext.cs
  Configurations/
External/
  Binance/
  TwelveData/
Repositories/
```

Notes:

- `Persistence/` is for EF Core and PostgreSQL
- `External/` is for HTTP clients and source-specific mapping
- `Repositories/` should exist only if they are actually useful

## Suggested Configuration Shape

Use one canonical instrument format inside the app:

- `BTCUSDT`
- `ETHUSDT`
- `EURUSD`
- `XAUUSD`

Source-specific symbol formatting should stay inside integration code. For example, Twelve Data may require a different symbol representation than Binance.

Example configuration:

```json
{
  "ConnectionStrings": {
    "Postgres": "Host=localhost;Port=5432;Database=market_feed_monitor;Username=postgres;Password=postgres"
  },
  "MarketData": {
    "Sources": {
      "Binance": {
        "BaseUrl": "https://api.binance.com",
        "PollIntervalSeconds": 15,
        "Instruments": [ "BTCUSDT", "ETHUSDT" ]
      },
      "TwelveData": {
        "BaseUrl": "https://api.twelvedata.com",
        "ApiKey": "",
        "PollIntervalMinutes": 5,
        "Instruments": [ "EURUSD", "XAUUSD" ]
      }
    }
  },
  "Alerts": {
    "StaleFeedThresholdSeconds": 90,
    "ConsecutiveFailureThreshold": 3
  }
}
```

## Suggested Configuration Classes

Keep configuration boring and explicit:

- `ConnectionStrings:Postgres`
  Used directly by EF Core and `MarketFeedMonitorDbContext`
- `MarketData:Sources:Binance`
  Bind to `BinanceOptions`
- `MarketData:Sources:TwelveData`
  Bind to `TwelveDataOptions`
- `Alerts`
  Bind to `AlertOptions`

Suggested API configuration classes:

- `MarketFeedMonitor.Api.Configuration.BinanceOptions`
- `MarketFeedMonitor.Api.Configuration.TwelveDataOptions`
- `MarketFeedMonitor.Api.Configuration.AlertOptions`

This is enough for the MVP. There is no need for a generic configuration framework or a deep options hierarchy.

## Why This Shape

- It keeps the project understandable.
- It separates the most obvious responsibilities without pretending the app is bigger than it is.
- It leaves room to grow later without forcing a rewrite on day one.

## Trade-Off

This structure is intentionally not perfect or future-proof.

That is the point.

If the project grows later, it will be easy to split more things out. Doing it now would mostly be architecture theater.
