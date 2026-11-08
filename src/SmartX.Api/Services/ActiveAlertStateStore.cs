using SmartX.Shared.Models;

namespace SmartX.Api.Services;

/// <summary>
/// Retains unique active error states between telemetry batches. A HashSet prevents
/// duplicate alert cards while the dictionary keeps a repeat count for the operator.
/// </summary>
public sealed class ActiveAlertStateStore
{
    private readonly object syncRoot = new();
    private readonly HashSet<string> activeAlertKeys = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, TrackedAlert> alertsByKey = new(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyList<TelemetryAlert> Synchronize(IEnumerable<TelemetryAlert> evaluatedAlerts)
    {
        var latestAlerts = evaluatedAlerts
            .GroupBy(CreateKey, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                group => group.Key,
                group => group.OrderByDescending(alert => alert.Severity)
                    .ThenByDescending(alert => alert.DetectedAtUtc)
                    .First(),
                StringComparer.OrdinalIgnoreCase);

        lock (syncRoot)
        {
            foreach (var (key, alert) in latestAlerts)
            {
                if (activeAlertKeys.Add(key))
                {
                    alertsByKey[key] = new TrackedAlert(alert, 1);
                    continue;
                }

                var existing = alertsByKey[key];
                alertsByKey[key] = new TrackedAlert(alert, existing.OccurrenceCount + 1);
            }

            var resolvedKeys = activeAlertKeys.Where(key => !latestAlerts.ContainsKey(key)).ToList();
            foreach (var key in resolvedKeys)
            {
                activeAlertKeys.Remove(key);
                alertsByKey.Remove(key);
            }

            return alertsByKey.Values
                .Select(tracked => ToAlert(tracked.Alert, tracked.OccurrenceCount))
                .OrderByDescending(alert => alert.Severity)
                .ThenByDescending(alert => alert.DetectedAtUtc)
                .ToList();
        }
    }

    public IReadOnlyList<TelemetryAlert> GetActiveAlerts()
    {
        lock (syncRoot)
        {
            return alertsByKey.Values
                .Select(tracked => ToAlert(tracked.Alert, tracked.OccurrenceCount))
                .OrderByDescending(alert => alert.Severity)
                .ThenByDescending(alert => alert.DetectedAtUtc)
                .ToList();
        }
    }

    private static string CreateKey(TelemetryAlert alert) => $"{alert.DeviceId}::{alert.MetricName}";

    private static TelemetryAlert ToAlert(TelemetryAlert alert, int occurrenceCount) => new()
    {
        DeviceId = alert.DeviceId,
        MetricName = alert.MetricName,
        Severity = alert.Severity,
        Message = alert.Message,
        SuggestedAction = alert.SuggestedAction,
        DetectedAtUtc = alert.DetectedAtUtc,
        OccurrenceCount = occurrenceCount
    };

    private sealed record TrackedAlert(TelemetryAlert Alert, int OccurrenceCount);
}
