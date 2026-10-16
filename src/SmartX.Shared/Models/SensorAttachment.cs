namespace SmartX.Shared.Models;

// Metadata for a file linked to one sensor profile.
public class SensorAttachment
{
    public string FileName { get; init; } = string.Empty;
    public string ContentType { get; init; } = "application/octet-stream";
    public long SizeBytes { get; init; }
    public DateTime UploadedAtUtc { get; init; }
}
