using System.Text.Json;
using System.Text.Json.Serialization;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using SmartX.Shared.Models;

namespace SmartX.Client.Services;

// Keeps HTTP calls out of the page so the user interface stays easy to read.
public class SensorApiClient(HttpClient httpClient)
{
    // The API sends enum values such as "Environmental" as text.
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public async Task<List<SensorProfile>> GetSensorsAsync()
    {
        return await httpClient.GetFromJsonAsync<List<SensorProfile>>("api/sensors/", JsonOptions) ?? [];
    }

    public Task<HttpResponseMessage> RegisterSensorAsync(SensorProfile sensor)
    {
        return httpClient.PostAsJsonAsync("api/sensors/", sensor, JsonOptions);
    }

    public async Task<TelemetryHistory> GetTelemetryHistoryAsync(string deviceId)
    {
        return await httpClient.GetFromJsonAsync<TelemetryHistory>(
            $"api/telemetry/{Uri.EscapeDataString(deviceId)}", JsonOptions) ?? new TelemetryHistory();
    }

    public async Task<List<TelemetryAlert>> GetAlertsAsync()
    {
        return await httpClient.GetFromJsonAsync<List<TelemetryAlert>>("api/alerts", JsonOptions) ?? [];
    }

    public async Task<NetworkTopologySnapshot> GetNetworkTopologyAsync()
    {
        return await httpClient.GetFromJsonAsync<NetworkTopologySnapshot>(
            "api/network/topology", JsonOptions) ?? new NetworkTopologySnapshot();
    }

    public async Task<NetworkConnectivityReport> GetNetworkConnectivityAsync(string startingDeviceId)
    {
        return await httpClient.GetFromJsonAsync<NetworkConnectivityReport>(
            $"api/network/connectivity/{Uri.EscapeDataString(startingDeviceId)}", JsonOptions)
            ?? new NetworkConnectivityReport { StartingDeviceId = startingDeviceId };
    }

    public async Task<MinimumSpanningTreeResult> GetMinimumSpanningTreeAsync(string startingDeviceId)
    {
        return await httpClient.GetFromJsonAsync<MinimumSpanningTreeResult>(
            $"api/network/minimum-spanning-tree/{Uri.EscapeDataString(startingDeviceId)}", JsonOptions)
            ?? new MinimumSpanningTreeResult { StartingDeviceId = startingDeviceId };
    }

    public Task<HttpResponseMessage> UpdateMeshNodeStatusAsync(string deviceId, bool isOnline)
    {
        return httpClient.PutAsJsonAsync(
            $"api/network/nodes/{Uri.EscapeDataString(deviceId)}/status",
            new MeshNodeStatusUpdate { IsOnline = isOnline },
            JsonOptions);
    }

    public async Task<List<CommandStreamTelemetry>> GetTelemetryRangeAsync(
        string deviceId,
        DateTime fromUtc,
        DateTime toUtc)
    {
        var from = Uri.EscapeDataString(fromUtc.ToUniversalTime().ToString("O"));
        var to = Uri.EscapeDataString(toUtc.ToUniversalTime().ToString("O"));

        return await httpClient.GetFromJsonAsync<List<CommandStreamTelemetry>>(
            $"api/telemetry/{Uri.EscapeDataString(deviceId)}/range?fromUtc={from}&toUtc={to}",
            JsonOptions) ?? [];
    }
    public async Task<List<CommandStreamTelemetry>> GetLiveCommandStreamAsync()
    {
        return await httpClient.GetFromJsonAsync<List<CommandStreamTelemetry>>(
            "api/command-stream/telemetry", JsonOptions) ?? [];
    }

    public async Task<List<TelemetryAlert>> GetPriorityCriticalAlertsAsync()
    {
        return await httpClient.GetFromJsonAsync<List<TelemetryAlert>>(
            "api/command-stream/critical-alerts", JsonOptions) ?? [];
    }

    public async Task<List<PredictiveInsight>> GetPredictiveInsightsAsync()
    {
        return await httpClient.GetFromJsonAsync<List<PredictiveInsight>>(
            "api/recommendations/insights", JsonOptions) ?? [];
    }

    public Task<HttpResponseMessage> RecordDeviceSearchAsync(string deviceId)
    {
        return httpClient.PostAsync($"api/recommendations/searches/{Uri.EscapeDataString(deviceId)}", null);
    }

    public Task<HttpResponseMessage> RunSimulationTickAsync()
    {
        return httpClient.PostAsync("api/simulation/tick", null);
    }

    public async Task<List<CommandStreamTelemetry>> GetSortedCommandHistoryAsync(string deviceId)
    {
        return await httpClient.GetFromJsonAsync<List<CommandStreamTelemetry>>(
            $"api/command-stream/history/{Uri.EscapeDataString(deviceId)}", JsonOptions) ?? [];
    }

    public async Task<List<ManualOverrideCommand>> GetManualOverrideHistoryAsync()
    {
        return await httpClient.GetFromJsonAsync<List<ManualOverrideCommand>>(
            "api/command-stream/commands", JsonOptions) ?? [];
    }

    public Task<HttpResponseMessage> IssueManualOverrideAsync(ManualOverrideRequest request)
    {
        return httpClient.PostAsJsonAsync("api/command-stream/commands", request, JsonOptions);
    }

    public async Task<HttpResponseMessage> UndoLatestManualOverrideAsync()
    {
        return await httpClient.PostAsync("api/command-stream/commands/undo", null);
    }

    public async Task<HttpResponseMessage> UploadAttachmentAsync(
        string deviceId,
        string fileName,
        string contentType,
        Stream fileStream)
    {
        using var form = new MultipartFormDataContent();
        using var fileContent = new StreamContent(fileStream);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(string.IsNullOrWhiteSpace(contentType)
            ? "application/octet-stream"
            : contentType);
        form.Add(fileContent, "file", fileName);

        return await httpClient.PostAsync(
            $"api/sensors/{Uri.EscapeDataString(deviceId)}/attachments",
            form);
    }

    public async Task<List<SensorAttachment>> GetAttachmentsAsync(string deviceId)
    {
        return await httpClient.GetFromJsonAsync<List<SensorAttachment>>(
            $"api/sensors/{Uri.EscapeDataString(deviceId)}/attachments", JsonOptions) ?? [];
    }
}
