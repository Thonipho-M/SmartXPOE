using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using SmartX.Client;
using SmartX.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var apiBaseUrl = builder.Configuration["SmartXApi:BaseUrl"]
    ?? throw new InvalidOperationException("SmartXApi:BaseUrl must be configured in wwwroot/appsettings.json.");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped(_ => new SensorApiClient(new HttpClient
{
    BaseAddress = new Uri(apiBaseUrl)
}));
builder.Services.AddScoped<SmartXWorkspaceState>();

await builder.Build().RunAsync();
