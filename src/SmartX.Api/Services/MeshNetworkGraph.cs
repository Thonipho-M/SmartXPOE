using SmartX.Shared.Models;

namespace SmartX.Api.Services;

// An adjacency-list graph: every device keeps a list of its direct wireless links.
public sealed class MeshNetworkGraph
{
    private readonly Dictionary<string, MeshNetworkNode> nodesById =
        new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, List<MeshNetworkLink>> linksByDeviceId =
        new(StringComparer.OrdinalIgnoreCase);
    private readonly List<MeshNetworkLink> links = [];

    public bool AddNode(MeshNetworkNode node)
    {
        if (!nodesById.TryAdd(node.DeviceId, node))
        {
            return false;
        }

        linksByDeviceId[node.DeviceId] = [];
        return true;
    }

    public bool AddLink(MeshNetworkLink link)
    {
        if (!nodesById.ContainsKey(link.FromDeviceId) ||
            !nodesById.ContainsKey(link.ToDeviceId) ||
            link.FromDeviceId.Equals(link.ToDeviceId, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        links.Add(link);
        linksByDeviceId[link.FromDeviceId].Add(link);
        linksByDeviceId[link.ToDeviceId].Add(link);
        return true;
    }

    public IReadOnlyList<MeshNetworkNode> GetNodes() => nodesById.Values.ToList();

    public IReadOnlyList<MeshNetworkLink> GetLinks() => links;

    public bool TryGetNode(string deviceId, out MeshNetworkNode? node) =>
        nodesById.TryGetValue(deviceId, out node);

    // Returns only usable, active routes from a device to online neighbours.
    public IReadOnlyList<MeshNetworkNode> GetActiveNeighbours(string deviceId)
    {
        if (!linksByDeviceId.TryGetValue(deviceId, out var deviceLinks))
        {
            return [];
        }

        return deviceLinks
            .Where(link => link.IsActive)
            .Select(link => link.FromDeviceId.Equals(deviceId, StringComparison.OrdinalIgnoreCase)
                ? link.ToDeviceId
                : link.FromDeviceId)
            .Select(neighbourId => nodesById[neighbourId])
            .Where(node => node.IsOnline)
            .ToList();
    }

    // Breadth-First Search (BFS) discovers every online node reachable through active links.
    public IReadOnlyList<MeshNetworkNode> GetReachableNodes(string startingDeviceId)
    {
        if (!TryGetNode(startingDeviceId, out var startingNode) || startingNode is null || !startingNode.IsOnline)
        {
            return [];
        }

        var visitedDeviceIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            startingNode.DeviceId
        };
        var nodesToVisit = new Queue<string>();
        var reachableNodes = new List<MeshNetworkNode>();

        nodesToVisit.Enqueue(startingNode.DeviceId);

        while (nodesToVisit.Count > 0)
        {
            var currentDeviceId = nodesToVisit.Dequeue();
            var currentNode = nodesById[currentDeviceId];
            reachableNodes.Add(currentNode);

            foreach (var neighbour in GetActiveNeighbours(currentDeviceId))
            {
                if (visitedDeviceIds.Add(neighbour.DeviceId))
                {
                    nodesToVisit.Enqueue(neighbour.DeviceId);
                }
            }
        }

        return reachableNodes;
    }
}
