namespace SmartX.Shared.Models;

// Summarises which mesh devices can currently communicate with a selected starting node.
public class NetworkConnectivityReport
{
    public string StartingDeviceId { get; init; } = string.Empty;

    public IReadOnlyList<MeshNetworkNode> ReachableNodes { get; init; } = [];

    public IReadOnlyList<MeshNetworkNode> UnreachableOnlineNodes { get; init; } = [];

    public IReadOnlyList<MeshNetworkNode> OfflineNodes { get; init; } = [];
}
