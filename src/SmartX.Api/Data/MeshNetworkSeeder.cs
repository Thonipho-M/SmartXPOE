using SmartX.Api.Services;
using SmartX.Shared.Models;

namespace SmartX.Api.Data;

// Creates a repeatable Smart-X mesh with alternate paths for routing demonstrations.
public static class MeshNetworkSeeder
{
    public static void Seed(MeshNetworkGraph graph, IEnumerable<SensorProfile> sensors)
    {
        graph.AddNode(new MeshNetworkNode
        {
            DeviceId = "SMARTX-GATEWAY-01",
            MacAddress = "AA:BB:CC:FF:00:01",
            Location = "Smart-X Central Gateway",
            Role = MeshNodeRole.Gateway
        });

        foreach (var sensor in sensors)
        {
            graph.AddNode(new MeshNetworkNode
            {
                DeviceId = sensor.DeviceId,
                MacAddress = sensor.MacAddress,
                Location = sensor.DeploymentLocation,
                Role = sensor.DeviceId is "FARM-PUMP-01" or "GRID-RELAY-01"
                    ? MeshNodeRole.Repeater
                    : MeshNodeRole.Sensor
            });
        }

        AddLink(graph, "SMARTX-GATEWAY-01", "FARM-PUMP-01", 2);
        AddLink(graph, "SMARTX-GATEWAY-01", "GRID-RELAY-01", 2);
        AddLink(graph, "SMARTX-GATEWAY-01", "GREENHOUSE-HUMID-01", 4);
        AddLink(graph, "FARM-PUMP-01", "FARM-VALVE-01", 1);
        AddLink(graph, "FARM-PUMP-01", "FARM-MOIST-01", 2);
        AddLink(graph, "FARM-VALVE-01", "FARM-TEMP-02", 2);
        AddLink(graph, "FARM-TEMP-02", "FARM-PH-01", 2);
        AddLink(graph, "FARM-PH-01", "FARM-MOIST-01", 3);
        AddLink(graph, "GRID-RELAY-01", "GRID-METER-01", 1);
        AddLink(graph, "GRID-RELAY-01", "GREENHOUSE-HUMID-01", 5);
    }

    private static void AddLink(MeshNetworkGraph graph, string fromDeviceId, string toDeviceId, int routingCost)
    {
        graph.AddLink(new MeshNetworkLink
        {
            FromDeviceId = fromDeviceId,
            ToDeviceId = toDeviceId,
            RoutingCost = routingCost
        });
    }
}
