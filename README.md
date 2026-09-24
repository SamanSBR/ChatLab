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

Browser-specific collection will eventually be a narrow JavaScript adapter that sends only browser APIs' legitimately available telemetry to an application service. OBS, v4l2loopback, computer vision, and other integrations will likewise live behind adapters; none is implemented in this baseline.

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

## Safety boundary

Only use data and sessions that are legitimately supplied to ChatLab and controlled by you or participants who have provided appropriate consent. This project must not be extended to bypass TLS/certificate pinning, authentication, bans, moderation, privacy controls, or site access controls.
