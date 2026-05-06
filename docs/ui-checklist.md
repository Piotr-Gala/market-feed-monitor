# MVP UI Checklist

This document turns the screen description into a practical MVP UI checklist.

Rule:

- do not add new screens until the MVP works end-to-end

## Screen 1: Dashboard

Purpose:

- show the current system state at a glance

UI checklist:

- [ ] summary cards section is visible
- [ ] active alerts count card is visible
- [ ] healthy feeds count card is visible
- [ ] stale feeds count card is visible
- [ ] down feeds count card is visible
- [ ] tracked instruments count card is visible
- [ ] instruments table is visible
- [ ] instruments table shows `symbol`
- [ ] instruments table shows `name`
- [ ] instruments table shows `asset type`
- [ ] instruments table shows `latest price`
- [ ] instruments table shows `last update`
- [ ] instruments table shows `1h change %`
- [ ] instruments table shows `feed status`
- [ ] instruments table shows `data source`
- [ ] active alerts list is visible
- [ ] active alerts list shows `instrument symbol`
- [ ] active alerts list shows `alert type`
- [ ] active alerts list shows `severity`
- [ ] active alerts list shows `message`
- [ ] active alerts list shows `created at`

Text wireframe:

```text
+---------------------------------------------------------------+
| Dashboard                                                     |
+---------------------------------------------------------------+
| [Active Alerts] [Healthy Feeds] [Stale Feeds] [Down Feeds]    |
| [Tracked Instruments]                                         |
+---------------------------------------------------------------+
| Instruments                                                   |
| Symbol | Name | Type | Latest Price | Last Update | 1h | ... |
| BTCUSDT | Bitcoin | Crypto | ...                             |
| ETHUSDT | Ethereum | Crypto | ...                            |
| EURUSD  | Euro / US Dollar | FX | ...                        |
| XAUUSD  | Gold / US Dollar | Commodity | ...                 |
+---------------------------------------------------------------+
| Active Alerts                                                 |
| Symbol | Type | Severity | Message | Created At              |
+---------------------------------------------------------------+
```

Suggested components:

- `SummaryCard`
- `InstrumentTable`
- `AlertList`
- `StatusBadge`

## Screen 2: Instrument Details

Purpose:

- show detailed data for one selected instrument

UI checklist:

- [ ] instrument header is visible
- [ ] instrument header shows `symbol`
- [ ] instrument header shows `name`
- [ ] instrument header shows `asset type`
- [ ] instrument header shows `latest price`
- [ ] instrument header shows `last update`
- [ ] instrument header shows `current feed status`
- [ ] instrument header shows `data source`
- [ ] 24h price chart is visible
- [ ] recent snapshots table is visible
- [ ] recent snapshots table shows `source timestamp`
- [ ] recent snapshots table shows `received at`
- [ ] recent snapshots table shows `price`
- [ ] recent snapshots table shows `data source`
- [ ] active alerts list is visible
- [ ] active alerts list shows `alert type`
- [ ] active alerts list shows `severity`
- [ ] active alerts list shows `message`
- [ ] active alerts list shows `created at`

Text wireframe:

```text
+---------------------------------------------------------------+
| Instrument Details                                            |
+---------------------------------------------------------------+
| BTCUSDT | Bitcoin | Crypto | 67250.12 | Healthy | Binance    |
| Last update: 2026-04-21 16:30:15                             |
+---------------------------------------------------------------+
| 24h Price Chart                                               |
| [ simple line chart area ]                                    |
+---------------------------------------------------------------+
| Recent Snapshots                                              |
| Source Timestamp | Received At | Price | Source              |
+---------------------------------------------------------------+
| Active Alerts                                                 |
| Type | Severity | Message | Created At                       |
+---------------------------------------------------------------+
```

Suggested components:

- `InstrumentHeader`
- `PriceChart24h`
- `SnapshotTable`
- `AlertList`
- `StatusBadge`

## Screen 3: Feed Health

Purpose:

- monitor the operational status of external data sources

UI checklist:

- [ ] feed health table is visible
- [ ] table shows `source name`
- [ ] table shows `is active`
- [ ] table shows `last successful fetch`
- [ ] table shows `consecutive failures`
- [ ] table shows `status`

Text wireframe:

```text
+---------------------------------------------------------------+
| Feed Health                                                   |
+---------------------------------------------------------------+
| Source       | Is Active | Last Successful Fetch | Failures   |
| Binance      | Yes       | 2026-04-21 16:30:15   | 0          |
| Twelve Data  | Yes       | 2026-04-21 16:25:00   | 0          |
| Status: Healthy / Stale / Down                               |
+---------------------------------------------------------------+
```

Suggested components:

- `FeedHealthTable`
- `StatusBadge`

## Optional But Sensible UI Support

These items are not required to ship the MVP, but they are practical and cheap:

- status badges:
  `Healthy` = green
  `Stale` = yellow
  `Down` = red

## Backend Data Note

The current health model is enough for MVP:

- `LastSuccessfulFetch`
- `ConsecutiveFailures`
- `Status`

One extra field would be very useful later:

- `LastAttemptAt`

Why it helps:

- it shows whether the worker is still running
- it separates "worker is dead" from "worker is alive but fetch is failing"

This field is recommended, but not required for the first end-to-end version.
