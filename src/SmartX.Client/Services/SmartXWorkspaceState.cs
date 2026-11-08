namespace SmartX.Client.Services;

/// <summary>
/// Keeps the operator's current investigation context while they move between
/// the connected Smart-X workspaces during a browser session.
/// </summary>
public sealed class SmartXWorkspaceState
{
    public string SelectedDeviceId { get; set; } = string.Empty;

    public string SelectedCategory { get; set; } = string.Empty;

    public bool ActiveAlertsOnly { get; set; }

    public string TopologySearchText { get; set; } = string.Empty;

    public string StartingMeshNodeId { get; set; } = "SMARTX-GATEWAY-01";
}
