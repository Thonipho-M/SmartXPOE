namespace SmartX.Shared.Models;

// A single response containing the complete mesh graph for the topology dashboard.
public class NetworkTopologySnapshot
{
    public IReadOnlyList<MeshNetworkNode> Nodes { get; init; } = [];

    public IReadOnlyList<MeshNetworkLink> Links { get; init; } = [];
}
