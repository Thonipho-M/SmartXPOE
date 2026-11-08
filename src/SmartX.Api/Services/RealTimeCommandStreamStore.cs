using SmartX.Shared.Models;

namespace SmartX.Api.Services;

public sealed class RealTimeCommandStreamStore
{
    private const int MaximumLivePackets = 100;
    private readonly object syncRoot = new();
    private readonly Queue<CommandStreamTelemetry> incomingTelemetryQueue = new();
    private readonly List<CommandStreamTelemetry> processedTelemetry = [];
    private readonly PriorityQueue<TelemetryAlert, (int Severity, long MostRecentFirst)> alertPriorityQueue = new();

    // Queue<T> preserves first-in, first-out order for the live telemetry stream.
    public void EnqueueTelemetry<T>(TelemetryPacket<T> packet)
    {
        var streamPacket = new CommandStreamTelemetry
        {
            DeviceId = packet.DeviceId,
            MetricName = packet.MetricName,
            Value = packet.Value?.ToString() ?? string.Empty,
            ValueType = typeof(T).Name,
            RecordedAtUtc = packet.RecordedAtUtc
        };

        lock (syncRoot)
        {
            incomingTelemetryQueue.Enqueue(streamPacket);
        }
    }

    // Consumes packets in the same first-in, first-out order in which the API received them.
    public int ProcessQueuedTelemetry()
    {
        lock (syncRoot)
        {
            var processedCount = 0;

            while (incomingTelemetryQueue.TryDequeue(out var packet))
            {
                processedTelemetry.Add(packet);
                processedCount++;
            }

            if (processedTelemetry.Count > MaximumLivePackets)
            {
                processedTelemetry.RemoveRange(0, processedTelemetry.Count - MaximumLivePackets);
            }

            return processedCount;
        }
    }

    // Alerts use severity first and newest timestamp second. Critical events therefore bypass normal telemetry work.
    public void UpdateAlertPriorities(IEnumerable<TelemetryAlert> alerts)
    {
        lock (syncRoot)
        {
            alertPriorityQueue.Clear();

            foreach (var alert in alerts)
            {
                var severityPriority = alert.Severity == AlertSeverity.Critical ? 0 : 1;
                var newestFirst = -alert.DetectedAtUtc.ToUniversalTime().Ticks;
                alertPriorityQueue.Enqueue(alert, (severityPriority, newestFirst));
            }
        }
    }

    public IReadOnlyList<CommandStreamTelemetry> GetLiveTelemetry()
    {
        lock (syncRoot)
        {
            return processedTelemetry.ToList();
        }
    }

    public IReadOnlyList<TelemetryAlert> GetCriticalAlerts()
    {
        lock (syncRoot)
        {
            var copiedQueue = new PriorityQueue<TelemetryAlert, (int Severity, long MostRecentFirst)>(
                alertPriorityQueue.UnorderedItems);
            var alerts = new List<TelemetryAlert>();

            while (copiedQueue.TryDequeue(out var alert, out _))
            {
                if (alert.Severity == AlertSeverity.Critical)
                {
                    alerts.Add(alert);
                }
            }

            return alerts;
        }
    }
}
