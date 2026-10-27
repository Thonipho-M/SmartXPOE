namespace SmartX.Shared.Models;

// Request used by the topology dashboard to simulate a device recovering or going offline.
public class MeshNodeStatusUpdate
{
    public bool IsOnline { get; init; }
}
