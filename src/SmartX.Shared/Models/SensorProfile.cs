namespace SmartX.Shared.Models;

// Describes one registered physical sensor or device.
public class SensorProfile
{
    // A Smart-X identifier such as "SENSOR-001".
    public string DeviceId { get; set; } = string.Empty;

    // The device network address, for example "AA:BB:CC:DD:EE:FF".
    public string MacAddress { get; set; } = string.Empty;

    // Where the device is installed, for example "Hydroponic Farm A - Zone 1".
    public string DeploymentLocation { get; set; } = string.Empty;

    // The kind of telemetry or action that the device provides.
    public SensorCategory Category { get; set; }
}
