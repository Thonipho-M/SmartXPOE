namespace SmartX.Shared.Models;

public class DeploymentValidationResult
{
    public bool IsValid => Issues.Count == 0;

    public List<string> Issues { get; set; } = [];
}
