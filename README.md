# ChatLab

ChatLab is a local research and experimentation toolkit for **consented, controlled** browser-based random video-chat sessions. It supports passive observation and later comparison of normal and anomalous session telemetry. It does not circumvent authorization, safety systems, privacy controls, or browser security.

## Architecture

```text
Blazor dashboard (ChatLab.Web)
        │
Application contracts and analysis use cases (ChatLab.Application)
        │
Domain entities (ChatLab.Domain)  ←  SQLite / EF Core adapters (ChatLab.Infrastructure)
```

`ChatLab.Domain` contains durable concepts: `ResearchSession` and its ordered `WebRtcSample` observations. `ChatLab.Application` owns the repository boundary so analysis and dashboard code never depend on a database implementation. `ChatLab.Infrastructure` implements that boundary with EF Core and SQLite. `ChatLab.Web` is the ASP.NET Core Blazor composition root and minimal dashboard.

The browser collector is a narrow, generic JavaScript adapter at `wwwroot/webrtc-telemetry.js`. It only observes an existing `RTCPeerConnection` explicitly supplied by a controlled page through `getStats()`; it does not create, modify, replay, or inspect signaling. It sends typed, best-effort stats to the ASP.NET endpoint, which normalizes browser units and stores them in SQLite. Candidate addresses and SDP are never collected.

## Projects

- `src/ChatLab.Domain` — `ResearchSession`, `WebRtcSample`, invariants.
- `src/ChatLab.Application` — cross-layer contracts.
- `src/ChatLab.Infrastructure` — EF Core SQLite context and repository.
- `src/ChatLab.Web` — Blazor Server dashboard; creates a local `chatlab.db` on first launch.
- `tests/ChatLab.Domain.Tests` — unit tests for domain invariants.

## Run

```bash
dotnet test
dotnet run --project src/ChatLab.Web
```

The dashboard uses `Data Source=chatlab.db`, resolved from the web project's working directory when running the web project.

## Local collector

Start the dashboard, create a consented session, and copy its returned `id`:

```bash
curl -k https://localhost:7252/api/sessions -H 'content-type: application/json' -d '{"label":"controlled local test"}'
```

In the JavaScript console of a browser page you control, after that page has created its own `RTCPeerConnection` named `pc`, load the collector and pass that existing connection to it:

```js
const stopTelemetry = ChatLabWebRtcTelemetry.start(pc, {
  endpoint: 'https://localhost:7252/api/sessions/SESSION_ID/webrtc-samples', intervalMs: 1000
});
// Later: stopTelemetry();
```

Include `https://localhost:7252/webrtc-telemetry.js` as a normal script in that controlled page. For another origin, add its exact origin under `Collector:AllowedOrigins` in `appsettings.json` and restart. Unavailable browser report fields are shown as `unavailable` in the dashboard.

## Safety boundary

Only use data and sessions that are legitimately supplied to ChatLab and controlled by you or participants who have provided appropriate consent. This project must not be extended to bypass TLS/certificate pinning, authentication, bans, moderation, privacy controls, or site access controls.
