namespace SmartX.Api.Services;

/// <summary>
/// Applies the same allow-list and size limits to every uploaded sensor attachment.
/// Files are kept outside the public web root and are stored with generated names.
/// </summary>
public static class AttachmentUploadValidator
{
    public const long MaximumFileSizeBytes = 10 * 1024 * 1024;

    private static readonly Dictionary<string, string[]> AllowedTypesByExtension = new(StringComparer.OrdinalIgnoreCase)
    {
        [".txt"] = ["text/plain"],
        [".log"] = ["text/plain"],
        [".json"] = ["application/json", "text/json"],
        [".xml"] = ["application/xml", "text/xml"],
        [".jpg"] = ["image/jpeg"],
        [".jpeg"] = ["image/jpeg"],
        [".png"] = ["image/png"],
        [".pdf"] = ["application/pdf"]
    };

    public static bool TryValidate(string? fileName, string? contentType, long length, out string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            errorMessage = "Choose a file to attach.";
            return false;
        }

        if (length <= 0 || length > MaximumFileSizeBytes)
        {
            errorMessage = "Attachments must be larger than 0 bytes and no more than 10 MB.";
            return false;
        }

        var extension = Path.GetExtension(fileName);
        if (!AllowedTypesByExtension.TryGetValue(extension, out var permittedContentTypes))
        {
            errorMessage = "Only TXT, LOG, JSON, XML, JPG, PNG, and PDF attachments are allowed.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(contentType) || !permittedContentTypes.Contains(contentType, StringComparer.OrdinalIgnoreCase))
        {
            errorMessage = $"The selected {extension} file has an unexpected content type.";
            return false;
        }

        errorMessage = string.Empty;
        return true;
    }

    public static string GetContentType(string fileName)
    {
        var extension = Path.GetExtension(fileName);
        return AllowedTypesByExtension.TryGetValue(extension, out var permittedContentTypes)
            ? permittedContentTypes[0]
            : "application/octet-stream";
    }
}
