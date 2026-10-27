using SmartX.Shared.Models;

namespace SmartX.Api.Services;

// A Binary Search Tree keyed by timestamp for efficient per-device telemetry range queries.
public sealed class TelemetryRangeSearchTree
{
    private readonly object syncRoot = new();
    private readonly Dictionary<string, TimestampNode?> rootsByDeviceId =
        new(StringComparer.OrdinalIgnoreCase);

    public void Add<T>(TelemetryPacket<T> packet)
    {
        var reading = new CommandStreamTelemetry
        {
            DeviceId = packet.DeviceId,
            MetricName = packet.MetricName,
            Value = packet.Value?.ToString() ?? string.Empty,
            ValueType = typeof(T).Name,
            RecordedAtUtc = packet.RecordedAtUtc.ToUniversalTime()
        };

        lock (syncRoot)
        {
            rootsByDeviceId.TryGetValue(packet.DeviceId, out var root);
            rootsByDeviceId[packet.DeviceId] = Insert(root, reading);
        }
    }

    public IReadOnlyList<CommandStreamTelemetry> GetRange(
        string deviceId,
        DateTime fromUtc,
        DateTime toUtc)
    {
        if (fromUtc > toUtc)
        {
            return [];
        }

        lock (syncRoot)
        {
            if (!rootsByDeviceId.TryGetValue(deviceId, out var root))
            {
                return [];
            }

            var matchingReadings = new List<CommandStreamTelemetry>();
            CollectRange(root, fromUtc.ToUniversalTime(), toUtc.ToUniversalTime(), matchingReadings);
            return matchingReadings;
        }
    }

    private static TimestampNode Insert(TimestampNode? node, CommandStreamTelemetry reading)
    {
        if (node is null)
        {
            return new TimestampNode(reading.RecordedAtUtc, [reading]);
        }

        if (reading.RecordedAtUtc < node.TimestampUtc)
        {
            node.Left = Insert(node.Left, reading);
        }
        else if (reading.RecordedAtUtc > node.TimestampUtc)
        {
            node.Right = Insert(node.Right, reading);
        }
        else
        {
            // Several metrics may arrive at the exact same timestamp for one device.
            node.Readings.Add(reading);
        }

        return node;
    }

    // In-order recursion skips branches that fall completely outside the requested time range.
    private static void CollectRange(
        TimestampNode? node,
        DateTime fromUtc,
        DateTime toUtc,
        List<CommandStreamTelemetry> matchingReadings)
    {
        if (node is null)
        {
            return;
        }

        if (node.TimestampUtc > fromUtc)
        {
            CollectRange(node.Left, fromUtc, toUtc, matchingReadings);
        }

        if (node.TimestampUtc >= fromUtc && node.TimestampUtc <= toUtc)
        {
            matchingReadings.AddRange(node.Readings);
        }

        if (node.TimestampUtc < toUtc)
        {
            CollectRange(node.Right, fromUtc, toUtc, matchingReadings);
        }
    }

    private sealed class TimestampNode(DateTime timestampUtc, List<CommandStreamTelemetry> readings)
    {
        public DateTime TimestampUtc { get; } = timestampUtc;
        public List<CommandStreamTelemetry> Readings { get; } = readings;
        public TimestampNode? Left { get; set; }
        public TimestampNode? Right { get; set; }
    }
}
