namespace SmartX.Shared.Models;

// Groups a sensor's historical readings without losing their individual value types.
public class TelemetryHistory
{
    public List<TelemetryPacket<float>> FloatPackets { get; set; } = [];
    public List<TelemetryPacket<int>> IntegerPackets { get; set; } = [];
    public List<TelemetryPacket<bool>> BooleanPackets { get; set; } = [];
}
