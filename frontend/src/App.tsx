import { useEffect, useState } from 'react'
import './App.css'

const API_BASE_URL = 'http://localhost:5071/api/market-data'
const REFRESH_INTERVAL_MS = 15000

type FeedSummary = {
  trackedInstruments: number
  storedSnapshots: number
  activeAlerts: number
  feeds: {
    total: number
    healthy: number
    stale: number
    down: number
  }
  lastSnapshotReceivedAt: string | null
  lastSuccessfulFetchAt: string | null
}

type LatestSnapshot = {
  symbol: string
  name: string
  assetType: string
  source: string
  price: number
  change1hPercent: number | null
  sourceTimestamp: string
  receivedAt: string
}

type FeedStatus = {
  source: string
  isActive: boolean
  lastAttemptAt: string | null
  lastSuccessfulFetchAt: string | null
  consecutiveFailures: number
  status: string
}

type ActiveAlert = {
  id: number
  symbol: string
  alertType: string
  severity: string
  message: string
  createdAt: string
}

type DashboardData = {
  summary: FeedSummary
  snapshots: LatestSnapshot[]
  feedStatuses: FeedStatus[]
  activeAlerts: ActiveAlert[]
}

function App() {
  const [data, setData] = useState<DashboardData | null>(null)
  const [error, setError] = useState<string | null>(null)
  const [isLoading, setIsLoading] = useState(true)

  useEffect(() => {
    let isMounted = true

    async function loadDashboardData() {
      try {
        const [summaryResponse, snapshotsResponse, feedStatusesResponse, alertsResponse] =
          await Promise.all([
            fetch(`${API_BASE_URL}/feed-summary`),
            fetch(`${API_BASE_URL}/latest-snapshots`),
            fetch(`${API_BASE_URL}/feed-statuses`),
            fetch(`${API_BASE_URL}/active-alerts`),
          ])

        if (
          !summaryResponse.ok ||
          !snapshotsResponse.ok ||
          !feedStatusesResponse.ok ||
          !alertsResponse.ok
        ) {
          throw new Error('Dashboard API request failed.')
        }

        const [summary, snapshots, feedStatuses, activeAlerts] = await Promise.all([
          summaryResponse.json(),
          snapshotsResponse.json(),
          feedStatusesResponse.json(),
          alertsResponse.json(),
        ])

        if (isMounted) {
          setData({ summary, snapshots, feedStatuses, activeAlerts })
          setError(null)
        }
      } catch (error) {
        if (isMounted) {
          setError(error instanceof Error ? error.message : 'Unknown dashboard error.')
        }
      } finally {
        if (isMounted) {
          setIsLoading(false)
        }
      }
    }

    loadDashboardData()

    const intervalId = window.setInterval(loadDashboardData, REFRESH_INTERVAL_MS)

    return () => {
      isMounted = false
      window.clearInterval(intervalId)
    }
  }, [])

  if (isLoading) {
    return <main className="dashboard">Loading dashboard...</main>
  }

  if (error !== null) {
    return (
      <main className="dashboard">
        <h1>Market Feed Monitor</h1>
        <p className="error">{error}</p>
      </main>
    )
  }

  if (data === null) {
    return null
  }

  return (
    <main className="dashboard">
      <header className="dashboard-header">
        <div>
          <h1>Market Feed Monitor</h1>
          <p>Feed health, latest prices, and active alerts.</p>
        </div>
      </header>

      <section className="summary-grid" aria-label="Feed summary">
        <SummaryCard label="Active Alerts" value={data.summary.activeAlerts} tone="critical" />
        <SummaryCard label="Healthy Feeds" value={data.summary.feeds.healthy} tone="healthy" />
        <SummaryCard label="Stale Feeds" value={data.summary.feeds.stale} tone="warning" />
        <SummaryCard label="Down Feeds" value={data.summary.feeds.down} tone="critical" />
        <SummaryCard label="Tracked Instruments" value={data.summary.trackedInstruments} />
        <SummaryCard label="Stored Snapshots" value={data.summary.storedSnapshots} />
      </section>

      <section className="panel">
        <div className="panel-header">
          <h2>Instruments</h2>
          <span>{formatDate(data.summary.lastSnapshotReceivedAt)}</span>
        </div>

        <table>
          <thead>
            <tr>
              <th>Symbol</th>
              <th>Name</th>
              <th>Type</th>
              <th>Latest Price</th>
              <th>1h Change</th>
              <th>Source</th>
              <th>Feed Status</th>
              <th>Last Update</th>
            </tr>
          </thead>
          <tbody>
            {data.snapshots.map((snapshot) => (
              <tr key={`${snapshot.symbol}-${snapshot.source}`}>
                <td>{snapshot.symbol}</td>
                <td>{snapshot.name}</td>
                <td>{snapshot.assetType}</td>
                <td>{formatPrice(snapshot.price)}</td>
                <td>{formatPercent(snapshot.change1hPercent)}</td>
                <td>{snapshot.source}</td>
                <td>
                  <StatusBadge status={getFeedStatus(snapshot.source, data.feedStatuses)} />
                </td>
                <td>{formatDate(snapshot.receivedAt)}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </section>

      <section className="panel">
        <div className="panel-header">
          <h2>Feed Statuses</h2>
        </div>

        <table>
          <thead>
            <tr>
              <th>Source</th>
              <th>Status</th>
              <th>Active</th>
              <th>Failures</th>
              <th>Last Attempt</th>
              <th>Last Success</th>
            </tr>
          </thead>
          <tbody>
            {data.feedStatuses.map((feedStatus) => (
              <tr key={feedStatus.source}>
                <td>{feedStatus.source}</td>
                <td>
                  <StatusBadge status={feedStatus.status} />
                </td>
                <td>{feedStatus.isActive ? 'Yes' : 'No'}</td>
                <td>{feedStatus.consecutiveFailures}</td>
                <td>{formatDate(feedStatus.lastAttemptAt)}</td>
                <td>{formatDate(feedStatus.lastSuccessfulFetchAt)}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </section>

      <section className="panel">
        <div className="panel-header">
          <h2>Active Alerts</h2>
        </div>

        {data.activeAlerts.length === 0 ? (
          <p className="empty-state">No active alerts.</p>
        ) : (
          <table>
            <thead>
              <tr>
                <th>Symbol</th>
                <th>Type</th>
                <th>Severity</th>
                <th>Message</th>
                <th>Created At</th>
              </tr>
            </thead>
            <tbody>
              {data.activeAlerts.map((alert) => (
                <tr key={alert.id}>
                  <td>{alert.symbol}</td>
                  <td>{alert.alertType}</td>
                  <td>{alert.severity}</td>
                  <td>{alert.message}</td>
                  <td>{formatDate(alert.createdAt)}</td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </section>
    </main>
  )
}

function SummaryCard({
  label,
  value,
  tone = 'neutral',
}: {
  label: string
  value: number
  tone?: 'neutral' | 'healthy' | 'warning' | 'critical'
}) {
  return (
    <article className={`summary-card summary-card-${tone}`}>
      <span>{label}</span>
      <strong>{value}</strong>
    </article>
  )
}

function StatusBadge({ status }: { status: string }) {
  return <span className={`status-badge status-${status.toLowerCase()}`}>{status}</span>
}

function getFeedStatus(source: string, feedStatuses: FeedStatus[]) {
  return feedStatuses.find((feedStatus) => feedStatus.source === source)?.status ?? 'Unknown'
}

function formatDate(value: string | null) {
  if (value === null) {
    return '-'
  }

  return new Intl.DateTimeFormat('en-GB', {
    dateStyle: 'short',
    timeStyle: 'medium',
  }).format(new Date(value))
}

function formatPrice(value: number) {
  return new Intl.NumberFormat('en-US', {
    maximumFractionDigits: 6,
  }).format(value)
}

function formatPercent(value: number | null) {
  if (value === null) {
    return '-'
  }

  return `${value.toFixed(2)}%`
}

export default App
