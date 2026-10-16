namespace SmartX.Shared.Models;

// Describes a problem detected automatically from telemetry data.
public class TelemetryAlert
{
    public string DeviceId { get; init; } = string.Empty;
    public string MetricName { get; init; } = string.Empty;
    public AlertSeverity Severity { get; init; }
    public string Message { get; init; } = string.Empty;
    public string SuggestedAction { get; init; } = string.Empty;
    public DateTime DetectedAtUtc { get; init; } = DateTime.UtcNow;
}
