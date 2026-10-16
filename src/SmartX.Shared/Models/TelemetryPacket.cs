namespace SmartX.Shared.Models;

// T represents the telemetry value type: for example float, int or bool.
public class TelemetryPacket<T>
{
    // Identifies the device that sent this reading.
    public string DeviceId { get; init; } = string.Empty;

    // Names the measurement, for example "Soil Moisture" or "Valve Open".
    public string MetricName { get; init; } = string.Empty;

    // Keeps the original value type without converting it to object.
    public T Value { get; init; } = default!;

    // Records when the gateway received the telemetry packet in UTC time.
    public DateTime RecordedAtUtc { get; init; } = DateTime.UtcNow;
}
