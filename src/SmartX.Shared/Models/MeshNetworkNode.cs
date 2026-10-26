namespace SmartX.Shared.Models;

// Represents one physical IoT device that can participate in mesh routing.
public class MeshNetworkNode
{
    // Matches the Smart-X device identifier used throughout the application.
    public string DeviceId { get; init; } = string.Empty;

    // Unique network address used to locate a device in the mesh.
    public string MacAddress { get; init; } = string.Empty;

    // Indicates whether this node is the gateway, a sensor, or a relay point.
    public MeshNodeRole Role { get; init; }

    // Allows the topology page to visibly distinguish active and unavailable devices.
    public bool IsOnline { get; set; } = true;

    // Friendly location displayed beside the node in the topology dashboard.
    public string Location { get; init; } = string.Empty;
}
