namespace SmartX.Shared.Models;

// A deployment location may contain smaller nested locations, such as a farm, greenhouse and bay.
public class DeploymentConfiguration
{
    public string Name { get; set; } = string.Empty;

    public List<string> SensorDeviceIds { get; set; } = [];

    public List<DeploymentConfiguration> ChildLocations { get; set; } = [];
}
