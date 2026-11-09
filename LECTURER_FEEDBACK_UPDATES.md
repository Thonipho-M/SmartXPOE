# Smart-X Lecturer Feedback Updates

**Student:** Thonipho Mavhungu  
**Student number:** ST10121100  
**Module:** PROG7312  
**Project:** Smart-X Final Integrated System

## Purpose

This document records the improvements made to the final Smart-X application in response to the lecturer feedback received for Part 1 and Part 2. The final project is presented as one connected IoT management system rather than as separate Part 1, Part 2, and POE applications.

## Part 1 feedback addressed

### Dynamic dashboard engagement

**Feedback:** The dashboard engagement features worked well but could be more seamless and dynamic.

**Update made:** The application now keeps a shared investigation context while the operator moves between Telemetry, Command Stream, and Network Topology. A selected device, Command Stream filters, topology search text, and routing start node remain available during navigation. Alerts, device history, command controls, and topology investigation therefore form one workflow instead of disconnected pages.

### User interface clarity

**Feedback:** The interface was clear, but further improvements could make it more intuitive for users.

**Update made:** The interface no longer labels workspaces as Part 1, Part 2, or POE. The Gateway uses the clearer labels **Monitor**, **Control**, and **Route**. The "How to use" panel was rewritten for first-time users with a three-step flow: identify a problem, investigate it, then take or undo a safe action. It also explains each menu item in plain language and includes a safe practice simulation.

### Alerts and investigation

**Feedback:** The chosen engagement strategies should help users identify anomalies and sensor problems quickly.

**Update made:** Proactive alerts remain connected to their device investigation history. The active-alert system now displays a repeat count when the same issue is detected more than once, instead of producing duplicate alert cards. This makes an ongoing fault visible without overwhelming the user with repeated notifications.

## Part 2 feedback addressed

### Navigation and active pillar state

**Feedback:** "No state survives navigation. No network topology pillar."

**Update made:** A browser-session `SmartXWorkspaceState` service now preserves the current investigation as the user navigates. Network Topology and Mesh Routing is available from both the navigation menu and the Gateway. The final system links device monitoring, manual control, and network routing through the same registered devices and shared state.

### FIFO queue and priority queue processing

**Feedback:** "Nothing consumes the queues. The critical lane does not bypass anything. Priority is based on recency and not severity."

**Update made:** Incoming telemetry now enters a `Queue<T>` and is actively consumed in first-in, first-out order into the bounded live command stream. The stream retains only the newest 100 processed packets. Alerts are placed in a `PriorityQueue<TelemetryAlert, ...>` that ranks **critical severity first** and uses timestamp only as a tie-breaker. Alert priorities are refreshed before ordinary telemetry is consumed, allowing critical conditions to appear immediately in the Critical Alerts area.

### Unique collections and alert duplication

**Feedback:** "No persistent unique set of error states. No duplicate-suppression count."

**Update made:** `ActiveAlertStateStore` uses a persistent `HashSet<string>` keyed by device and problem type, along with a dictionary of alert details. A repeated fault increments `OccurrenceCount` rather than creating another alert. When a healthy latest reading replaces the abnormal reading, the alert key is removed automatically. Critical Alerts and Automated Insights read from this same active state, so both areas clear the resolved issue together.

### Predictive Action and Suggestion Engine

**Feedback:** "The main suggestions are hard-coded rules, not learned. Learned patterns are raw counts."

**Update made:** The recommendation engine now records ordered operator behaviour, such as viewing an alerted sensor followed by sending a manual override to another device. Each learned follow-up pattern stores:

- source and target device;
- number of observed sequences;
- number of sequences that ended in a manual override;
- most recent observation time; and
- the command and target state used in the latest manual response.

Automated Insights rank patterns using frequency, manual-override evidence, and recency. Each suggestion explains its confidence and evidence. If no matching operator pattern has been learned, the dashboard states that the insight is alert-based rather than presenting it as a learned prediction.

## Final integrated features supporting the feedback

- **Telemetry:** device registration, typed telemetry, attachments, timestamps, alerts, and reading investigation.
- **Command Stream:** live FIFO telemetry processing, severity-first critical alert queue, command history, manual override, undo, and learned suggestions.
- **Network Topology:** mesh graph, device/MAC search, connectivity analysis, failure simulation, minimum-cost routing, and timestamp-range history queries.
- **Shared experience:** a user can investigate one device across the monitoring, control, and network screens without losing their selected context.

## Remaining improvement work

The following Part 1 feedback items are recorded as future improvements and are not claimed as completed by this document:

- Move the API base address from client code into configuration.
- Add stronger MAC-address format validation and telemetry value range validation.
- Add additional security controls for uploaded files and stored configuration data.
- Add explicit upload size/type validation feedback at both client and API levels.

## Evidence

The above improvements are implemented in the Smart-X source code and are supported by focused Git commits with descriptive messages. The project README contains setup and run instructions for the integrated solution.
