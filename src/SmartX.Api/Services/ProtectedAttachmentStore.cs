using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;

namespace SmartX.Api.Services;

/// <summary>
/// Protects the contents of accepted attachments before they are written to disk.
/// The built-in ASP.NET Core data-protection system supplies encryption and integrity checks.
/// </summary>
public sealed class ProtectedAttachmentStore(IDataProtectionProvider dataProtectionProvider)
{
    private readonly IDataProtector protector = dataProtectionProvider.CreateProtector("SmartX.SensorAttachments.v1");

    public async Task StoreAsync(IFormFile uploadedFile, string destinationPath, CancellationToken cancellationToken = default)
    {
        await using var source = uploadedFile.OpenReadStream();
        await using var buffer = new MemoryStream();
        await source.CopyToAsync(buffer, cancellationToken);

        var protectedBytes = protector.Protect(buffer.ToArray());
        await using var destination = new FileStream(
            destinationPath,
            FileMode.CreateNew,
            FileAccess.Write,
            FileShare.None,
            bufferSize: 81_920,
            useAsync: true);
        await destination.WriteAsync(protectedBytes, cancellationToken);
    }
}
