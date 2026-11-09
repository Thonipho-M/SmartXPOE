using SmartX.Shared.Models;

namespace SmartX.Api.Services;

public sealed class OperatorActivityTracker
{
    private const int MaximumActivityEvents = 50;
    private readonly object syncRoot = new();
    private readonly Queue<OperatorActivity> recentActivities = new();
    private readonly Dictionary<string, Dictionary<string, FollowUpPatternStats>> followUpPatterns =
        new(StringComparer.OrdinalIgnoreCase);
    private OperatorActivity? previousActivity;

    public void RecordSearch(string deviceId)
    {
        Record(deviceId, OperatorActivityKind.Search, "Viewed device history");
    }

    public void RecordManualOverride(ManualOverrideCommand command)
    {
        Record(command.DeviceId, OperatorActivityKind.ManualOverride, command.Command, command.TargetState);
    }

    public IReadOnlyList<OperatorActivity> GetRecentActivities()
    {
        lock (syncRoot)
        {
            return recentActivities.Reverse().ToList();
        }
    }

    public IReadOnlyList<string> GetFrequentFollowUps(string deviceId)
    {
        return GetFollowUpPatterns(deviceId)
            .OrderByDescending(pattern => pattern.OccurrenceCount)
            .Select(pattern => pattern.TargetDeviceId)
            .ToList();
    }

    public IReadOnlyList<OperatorFollowUpPattern> GetFollowUpPatterns(string deviceId)
    {
        lock (syncRoot)
        {
            if (!followUpPatterns.TryGetValue(deviceId, out var relatedDevices))
            {
                return [];
            }

            return relatedDevices
                .Select(entry => new OperatorFollowUpPattern
                {
                    SourceDeviceId = deviceId,
                    TargetDeviceId = entry.Key,
                    OccurrenceCount = entry.Value.OccurrenceCount,
                    ManualOverrideCount = entry.Value.ManualOverrideCount,
                    LastObservedAtUtc = entry.Value.LastObservedAtUtc,
                    RecommendedCommand = entry.Value.LastCommand,
                    RecommendedTargetState = entry.Value.LastTargetState
                })
                .OrderByDescending(pattern => pattern.OccurrenceCount)
                .ThenBy(pattern => pattern.TargetDeviceId)
                .ToList();
        }
    }

    // A nested dictionary retains ordered patterns such as "Moisture Sensor B" followed by "Pump A".
    private void Record(string deviceId, OperatorActivityKind kind, string description, string? targetState = null)
    {
        lock (syncRoot)
        {
            if (previousActivity is not null &&
                !previousActivity.DeviceId.Equals(deviceId, StringComparison.OrdinalIgnoreCase))
            {
                if (!followUpPatterns.TryGetValue(previousActivity.DeviceId, out var relatedDevices))
                {
                    relatedDevices = new Dictionary<string, FollowUpPatternStats>(StringComparer.OrdinalIgnoreCase);
                    followUpPatterns[previousActivity.DeviceId] = relatedDevices;
                }

                var existing = relatedDevices.GetValueOrDefault(deviceId) ?? new FollowUpPatternStats();
                relatedDevices[deviceId] = existing with
                {
                    OccurrenceCount = existing.OccurrenceCount + 1,
                    ManualOverrideCount = existing.ManualOverrideCount + (kind == OperatorActivityKind.ManualOverride ? 1 : 0),
                    LastObservedAtUtc = DateTime.UtcNow,
                    LastCommand = kind == OperatorActivityKind.ManualOverride ? description : existing.LastCommand,
                    LastTargetState = kind == OperatorActivityKind.ManualOverride ? targetState : existing.LastTargetState
                };
            }

            var activity = new OperatorActivity
            {
                DeviceId = deviceId,
                Kind = kind,
                Description = description
            };
            recentActivities.Enqueue(activity);

            if (recentActivities.Count > MaximumActivityEvents)
            {
                recentActivities.Dequeue();
            }

            previousActivity = activity;
        }
    }

    private sealed record FollowUpPatternStats(
        int OccurrenceCount = 0,
        int ManualOverrideCount = 0,
        DateTime LastObservedAtUtc = default,
        string? LastCommand = null,
        string? LastTargetState = null);
}
