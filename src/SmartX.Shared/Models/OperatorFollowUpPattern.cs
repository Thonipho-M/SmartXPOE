namespace SmartX.Shared.Models;

// A learned ordered relationship between two operator actions.
public class OperatorFollowUpPattern
{
    public string SourceDeviceId { get; init; } = string.Empty;

    public string TargetDeviceId { get; init; } = string.Empty;

    public int OccurrenceCount { get; init; }

    public int ManualOverrideCount { get; init; }

    public DateTime LastObservedAtUtc { get; init; }

    public string? RecommendedCommand { get; init; }

    public string? RecommendedTargetState { get; init; }
}
