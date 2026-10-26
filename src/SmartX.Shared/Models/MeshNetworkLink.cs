namespace SmartX.Shared.Models;

// Represents one two-way wireless connection between devices in the IoT mesh.
public class MeshNetworkLink
{
    // The device at one end of the connection.
    public string FromDeviceId { get; init; } = string.Empty;

    // The device at the other end of the connection.
    public string ToDeviceId { get; init; } = string.Empty;

    // A lower value represents a stronger, lower-power wireless hop.
    public int RoutingCost { get; init; }

    // Shows whether this physical wireless connection is currently usable.
    public bool IsActive { get; set; } = true;
}
