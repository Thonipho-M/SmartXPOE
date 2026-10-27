namespace SmartX.Shared.Models;

// The lowest-cost set of active links that connects the reachable part of the mesh.
public class MinimumSpanningTreeResult
{
    public string StartingDeviceId { get; init; } = string.Empty;

    public IReadOnlyList<MeshNetworkLink> SelectedLinks { get; init; } = [];

    public int TotalRoutingCost { get; init; }

    // Online nodes listed here could not be reached by the active tree.
    public IReadOnlyList<string> UnconnectedDeviceIds { get; init; } = [];
}
