using System.Text.RegularExpressions;
using SmartX.Shared.Models;

namespace SmartX.Shared.Validation;

/// <summary>
/// Shared input checks used before device records and telemetry enter the Smart-X system.
/// </summary>
public static partial class SmartXInputValidator
{
    [GeneratedRegex("^[0-9A-Fa-f]{2}(:[0-9A-Fa-f]{2}){5}$")]
    private static partial Regex MacAddressPattern();

    public static bool IsValidMacAddress(string? macAddress) =>
        !string.IsNullOrWhiteSpace(macAddress) && MacAddressPattern().IsMatch(macAddress.Trim());

    public static bool TryValidateTelemetry<T>(TelemetryPacket<T> packet, out string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(packet.DeviceId) || string.IsNullOrWhiteSpace(packet.MetricName))
        {
            errorMessage = "Device ID and metric name are required.";
            return false;
        }

        if (packet.Value is float floatValue)
        {
            return TryValidateFloat(packet.MetricName, floatValue, out errorMessage);
        }

        if (packet.Value is int integerValue)
        {
            return TryValidateInteger(packet.MetricName, integerValue, out errorMessage);
        }

        errorMessage = string.Empty;
        return true;
    }

    private static bool TryValidateFloat(string metricName, float value, out string errorMessage)
    {
        if (float.IsNaN(value) || float.IsInfinity(value))
        {
            errorMessage = "Telemetry values must be finite numbers.";
            return false;
        }

        var range = metricName switch
        {
            "Soil Moisture" or "Humidity" => (Minimum: 0f, Maximum: 100f),
            "Nutrient pH" => (Minimum: 0f, Maximum: 14f),
            "Ambient Temperature" or "Nutrient Temperature" => (Minimum: -50f, Maximum: 100f),
            _ => (Minimum: -1_000_000f, Maximum: 1_000_000f)
        };

        if (value < range.Minimum || value > range.Maximum)
        {
            errorMessage = $"{metricName} must be between {range.Minimum} and {range.Maximum}.";
            return false;
        }

        errorMessage = string.Empty;
        return true;
    }

    private static bool TryValidateInteger(string metricName, int value, out string errorMessage)
    {
        var range = metricName switch
        {
            "Power Usage Watts" => (Minimum: 0, Maximum: 50_000),
            "Voltage" => (Minimum: 0, Maximum: 1_000),
            _ => (Minimum: -1_000_000, Maximum: 1_000_000)
        };

        if (value < range.Minimum || value > range.Maximum)
        {
            errorMessage = $"{metricName} must be between {range.Minimum} and {range.Maximum}.";
            return false;
        }

        errorMessage = string.Empty;
        return true;
    }
}
