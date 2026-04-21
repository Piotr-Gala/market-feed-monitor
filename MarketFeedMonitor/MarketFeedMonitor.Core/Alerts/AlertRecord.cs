using MarketFeedMonitor.Core.Instruments;

namespace MarketFeedMonitor.Core.Alerts;

public sealed class AlertRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid InstrumentId { get; set; }
    public AlertType AlertType { get; set; }
    public AlertSeverity Severity { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public bool IsActive { get; set; } = true;

    public InstrumentDefinition Instrument { get; set; } = null!;
}
