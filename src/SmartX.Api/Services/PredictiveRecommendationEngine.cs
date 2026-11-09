using SmartX.Shared.Models;

namespace SmartX.Api.Services;

public sealed class PredictiveRecommendationEngine(OperatorActivityTracker activityTracker)
{
    public IReadOnlyList<PredictiveInsight> Generate(
        IEnumerable<TelemetryAlert> activeAlerts,
        IEnumerable<SensorProfile> sensors)
    {
        var sensorList = sensors.ToList();
        var insights = new List<PredictiveInsight>();
        var handledDevices = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var alert in activeAlerts.Where(alert => alert.Severity == AlertSeverity.Critical))
        {
            if (!handledDevices.Add(alert.DeviceId))
            {
                continue;
            }

            var rankedPatterns = activityTracker.GetFollowUpPatterns(alert.DeviceId)
                .Where(pattern => sensorList.Any(sensor =>
                    sensor.DeviceId.Equals(pattern.TargetDeviceId, StringComparison.OrdinalIgnoreCase)))
                .OrderByDescending(GetPatternScore)
                .ToList();
            var learnedControl = rankedPatterns.FirstOrDefault(pattern =>
                pattern.ManualOverrideCount > 0 &&
                !string.IsNullOrWhiteSpace(pattern.RecommendedCommand) &&
                !string.IsNullOrWhiteSpace(pattern.RecommendedTargetState));

            insights.Add(new PredictiveInsight
            {
                DeviceId = alert.DeviceId,
                Title = $"Immediate attention: {alert.MetricName}",
                Message = alert.Message,
                SuggestedAction = learnedControl is null
                    ? alert.SuggestedAction
                    : $"{learnedControl.RecommendedCommand} on {learnedControl.TargetDeviceId} ({learnedControl.RecommendedTargetState}).",
                Confidence = learnedControl is null ? "Alert-based" : GetConfidence(GetPatternScore(learnedControl)),
                Evidence = learnedControl is null
                    ? "A critical telemetry alert is active. No matching operator response pattern has been learned yet."
                    : DescribePattern(learnedControl),
                RecommendedDeviceId = learnedControl?.TargetDeviceId,
                RecommendedCommand = learnedControl?.RecommendedCommand,
                RecommendedTargetState = learnedControl?.RecommendedTargetState
            });

            foreach (var pattern in rankedPatterns.Take(2))
            {
                var relatedDevice = sensorList.FirstOrDefault(sensor =>
                    sensor.DeviceId.Equals(pattern.TargetDeviceId, StringComparison.OrdinalIgnoreCase));

                if (relatedDevice is null)
                {
                    continue;
                }

                insights.Add(new PredictiveInsight
                {
                    DeviceId = relatedDevice.DeviceId,
                    Title = $"Likely follow-up device: {relatedDevice.DeviceId}",
                    Message = $"Operators previously moved from {alert.DeviceId} to this device after a related investigation.",
                    SuggestedAction = $"Review {relatedDevice.DeviceId} at {relatedDevice.DeploymentLocation} next.",
                    Confidence = GetConfidence(GetPatternScore(pattern)),
                    Evidence = DescribePattern(pattern),
                    RecommendedDeviceId = pattern.ManualOverrideCount > 0 ? relatedDevice.DeviceId : null,
                    RecommendedCommand = pattern.RecommendedCommand,
                    RecommendedTargetState = pattern.RecommendedTargetState
                });
            }
        }

        return insights;
    }

    private static int GetPatternScore(OperatorFollowUpPattern pattern)
    {
        var age = DateTime.UtcNow - pattern.LastObservedAtUtc;
        var recencyPoints = age <= TimeSpan.FromHours(1) ? 12 : age <= TimeSpan.FromDays(1) ? 6 : 2;
        return pattern.OccurrenceCount * 10 + pattern.ManualOverrideCount * 15 + recencyPoints;
    }

    private static string GetConfidence(int score) => score >= 45 ? "High" : score >= 25 ? "Medium" : "Emerging";

    private static string DescribePattern(OperatorFollowUpPattern pattern) =>
        $"Observed {pattern.OccurrenceCount} follow-up sequence(s), including {pattern.ManualOverrideCount} manual override(s); " +
        $"last seen {pattern.LastObservedAtUtc.ToLocalTime():dd MMM HH:mm}.";
}
