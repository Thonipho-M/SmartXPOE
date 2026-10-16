using SmartX.Shared.Models;

namespace SmartX.Api.Services;

public static class DeploymentConfigurationValidator
{
    public static DeploymentValidationResult Validate(
        DeploymentConfiguration configuration,
        IEnumerable<SensorProfile> registeredSensors)
    {
        var result = new DeploymentValidationResult();
        var registeredDeviceIds = registeredSensors
            .Select(sensor => sensor.DeviceId)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        ValidateLocation(configuration, "Deployment", registeredDeviceIds, result.Issues);
        return result;
    }

    // Recursion validates this location, then repeats the same process for every nested location.
    private static void ValidateLocation(
        DeploymentConfiguration location,
        string parentPath,
        HashSet<string> registeredDeviceIds,
        List<string> issues)
    {
        var displayName = string.IsNullOrWhiteSpace(location.Name)
            ? "Unnamed location"
            : location.Name.Trim();
        var currentPath = $"{parentPath} > {displayName}";

        if (string.IsNullOrWhiteSpace(location.Name))
        {
            issues.Add($"{parentPath} contains a location with no name.");
        }

        foreach (var deviceId in location.SensorDeviceIds)
        {
            if (string.IsNullOrWhiteSpace(deviceId))
            {
                issues.Add($"{currentPath} contains an empty sensor device ID.");
            }
            else if (!registeredDeviceIds.Contains(deviceId))
            {
                issues.Add($"{currentPath} references '{deviceId}', which is not a registered sensor.");
            }
        }

        foreach (var childLocation in location.ChildLocations)
        {
            ValidateLocation(childLocation, currentPath, registeredDeviceIds, issues);
        }
    }
}
