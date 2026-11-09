# Smart-X Final POE

Smart-X is a simulated Internet of Things management system for distributed environments such as hydroponic farms, smart-grid installations, and automated utility trackers. It provides one connected workspace for device monitoring, telemetry investigation, manual control, alerts, learned recommendations, and mesh-network routing.

## Project details

- **Developer:** Thonipho Mavhungu
- **Student number:** ST10121100
- **Module assessment:** PROG7312 Smart-X Final POE

## Implemented features

- Smart-X gateway with integrated **Monitor**, **Control**, and **Route** workspaces.
- ASP.NET Core Minimal API for sensor registration and telemetry ingestion.
- Sensor profiles containing device ID, MAC address, deployment location, and category.
- Client and API MAC-address validation using the `AA:BB:CC:DD:EE:FF` format.
- Generic `TelemetryPacket<T>` model for `float`, `int`, and `bool` telemetry values.
- Typed telemetry endpoints that preserve the original value type.
- Server-side telemetry-range validation for moisture, humidity, pH, temperature, power, and voltage readings.
- Mock sensor and telemetry seeding using jagged arrays before data is transferred to `List<T>` collections.
- Sensor registration form connected to the API, with immediate validation feedback.
- Timestamped telemetry history with metric filtering and reading drill-down.
- Proactive alerts for low soil moisture, power spikes, and sensor disconnects.
- Persistent unique alert tracking with repeat counts and automatic removal after a healthy reading.
- Alert investigation view with an actionable recommendation and linked sensor history.
- Beginner-friendly in-app help guide with a three-step first-use flow and safe practice simulation.
- Real-Time Command Stream displaying recent incoming telemetry and priority critical alerts.
- Device filtering by operational category and active alert state.
- Manual actuator overrides with a stack-based **Undo latest** feature.
- Timestamp-sorted historical telemetry for efficient timeline review.
- Predictive Automated Insights based on critical readings and learned, ordered operator search/command patterns.
- Attachment validation for TXT, LOG, JSON, XML, JPG, PNG, and PDF files up to 10 MB, with protected local file storage.
- Shared browser-session state so device context and filters persist between integrated workspaces.
- Network Topology dashboard with searchable device IDs, MAC addresses, connection map, and node-status simulation.
- Breadth-First Search connectivity reporting for reachable, isolated, and offline mesh devices.
- Prim's Minimum Spanning Tree route planner showing the lowest-cost active network links.
- Timestamp-range telemetry querying through a dedicated Binary Search Tree.

## Data structures and algorithms

Smart-X deliberately uses the following structures to manage telemetry, operator activity, and the final mesh network requirements:

- `Queue<T>` accepts incoming telemetry and processes it in first-in, first-out order before it enters the bounded live stream.
- `PriorityQueue<TElement, TPriority>` ranks active alerts by severity first and timestamp second, allowing critical alerts to bypass routine telemetry processing.
- `Stack<T>` stores manual override commands so the most recent command can be undone first.
- `Dictionary<TKey, TValue>` stores live devices by unique device ID for fast lookups when telemetry or commands arrive.
- `SortedDictionary<TKey, TValue>` organises historical readings by UTC timestamp.
- `HashSet<T>` maintains unique active error states and prevents duplicate alert cards.
- A nested dictionary stores learned ordered device follow-ups, including frequency, recency, and manual override evidence.
- `MeshNetworkGraph` uses an adjacency-list graph to model two-way wireless mesh connections.
- Breadth-First Search uses `Queue<T>` and `HashSet<T>` to discover all devices reachable from a selected gateway.
- Prim's Minimum Spanning Tree uses `PriorityQueue<TElement, TPriority>` as a min-heap to select the lowest-cost active links.
- `TelemetryRangeSearchTree` is a timestamp-keyed Binary Search Tree that retrieves only readings inside a requested range.

## Technology stack

- .NET 10
- ASP.NET Core Minimal API
- Blazor WebAssembly
- C#
- Bootstrap and custom CSS

## Project structure

```text
src/
  SmartX.Api/       ASP.NET Core API and mock telemetry seeding
  SmartX.Client/    Blazor WebAssembly dashboard
  SmartX.Shared/    Shared models used by the API and dashboard
LECTURER_FEEDBACK_UPDATES.md  Record of improvements made from lecturer feedback
```

## Setup and running the project

### Prerequisites

Install the **.NET 10 SDK** before running the project. Confirm it is available by opening PowerShell and running:

```powershell
dotnet --version
```

The command should display a version starting with `10`.

### 1. Open the project folder

In PowerShell, move into the folder that contains `SmartX.slnx`:

```powershell
cd "C:\path\to\SmartXPOE"
```

### 2. Restore dependencies

Restore the NuGet packages for the complete solution. This is needed after cloning the repository or when dependencies change:

```powershell
dotnet restore SmartX.slnx
```

### 3. Confirm the API address

The client reads the API address from `src\SmartX.Client\wwwroot\appsettings.json`. For local testing, it should remain:

```json
{
  "SmartXApi": {
    "BaseUrl": "http://localhost:5145/"
  }
}
```

### 4. Compile the solution

Build both the API and the Blazor WebAssembly client:

```powershell
dotnet build SmartX.slnx
```

The build is successful when PowerShell reports `Build succeeded` with zero errors.

### 5. Start the backend API

Keep the first PowerShell window open and run:

```powershell
dotnet run --project src\SmartX.Api --launch-profile http
```

The API starts at `http://localhost:5145`. Leave this window running while using the dashboard.

### 6. Start the client dashboard

Open a **second** PowerShell window, move to the same project folder, and run:

```powershell
dotnet run --project src\SmartX.Client --launch-profile http
```

The browser should open automatically. If it does not, open [http://localhost:5025/](http://localhost:5025/) manually.

### 7. Stop the application

In each PowerShell window, press `Ctrl` + `C` to stop the API or client. The API uses temporary in-memory data, so newly registered sensors, uploaded attachments, command history, and learned activity patterns are cleared when the API stops.

## How to use the dashboard

### Telemetry workspace

1. Open **Telemetry** from the Smart-X gateway.
2. Review proactive alerts and select **Investigate** to open the affected sensor's history.
3. Register a sensor using its device ID, MAC address, deployment location, and category.
4. Use the MAC format `AA:BB:CC:DD:EE:FF`; invalid addresses are rejected before registration.
5. Attach a supporting configuration, log, or image file if needed. Only the displayed approved file types up to 10 MB are accepted.
6. Choose a sensor, apply a metric filter, then select a reading to view its timestamp and source device.

### Real-Time Command Stream

1. Open **Command stream** from the gateway or sidebar.
2. Use the category drop-down or **Show devices with active alerts only** option to narrow the device list.
3. Review the incoming telemetry table. It refreshes automatically every five seconds; **Refresh stream** performs an immediate update.
4. Review **Critical alerts** first, then read **Automated Insights** to see the suggested action and the evidence supporting it.
5. Select **Review device** on an insight to inspect its timestamp-sorted history. This interaction helps the system learn future related-device patterns.
6. Under **Manual override**, choose an actuator device, select a command and target state, then select **Issue command**.
7. Select **Undo latest** to reverse the most recently issued manual override. Commands are undone in reverse order.
8. When the matching safe simulation reading arrives, the related alert and insight automatically clear.

### Network Topology and Mesh Routing

1. Open **Network topology** from the sidebar.
2. Use the search field to find a device by ID, MAC address, role, or location.
3. Select a starting node, normally `SMARTX-GATEWAY-01`, then choose **Analyse connectivity** to run BFS.
4. Choose a non-gateway node under **Failure simulation**, then select **Toggle status** to show how the mesh reacts to a failed or recovered device.
5. Choose **Calculate efficient route** to run Prim's algorithm and inspect the minimum-spanning-tree links and total routing cost.
6. Select a device and timestamp range under **Historical telemetry range**, then choose **Find readings** to query the Binary Search Tree.

### First-time use

Select **How to use** in the upper-right corner for a short guided workflow: identify an alert in Command Stream, investigate it in Telemetry, then use a manual override only when appropriate. The same panel includes a safe node-failure simulation for the Network Topology page.

## AI assistance acknowledgement

This project was developed and reviewed by **Thonipho Mavhungu**. ChatGPT was used as an assistive tool during development, particularly for UI-layout guidance, code explanation, debugging support, and refining the project documentation. The final implementation was reviewed, tested, and organised by the student.
