# ChatLab engineering guide

## Safety and scope

- ChatLab is for passive observation and controlled testing with sessions/accounts legitimately available to the operator.
- Never implement or advise bypassing TLS or certificate pinning, authentication, authorization, bans, moderation, rate limits, or privacy controls.
- Do not collect private data that is not deliberately supplied to the client, impersonate people or moderators, interfere with other users, or automate deceptive behavior.
- Keep integrations opt-in, local-first, and narrowly scoped. Document consent and data-retention implications before introducing collection features.

## Architecture

- Use C# for application logic, persistence, analysis, and the dashboard.
- Restrict JavaScript to browser-only APIs, such as future WebRTC statistics collection.
- Keep service- and platform-specific code behind interfaces in `ChatLab.Application`, with implementations in `ChatLab.Infrastructure` or dedicated adapter projects.
- Domain projects must not reference EF Core, ASP.NET Core, JavaScript, OBS, or external services.
- Add tests for domain invariants and application behavior whenever changing them.

## Engineering practices

- Target .NET 10 and nullable reference types.
- Prefer small, explicit models and asynchronous I/O with cancellation tokens.
- Do not put secrets, account identifiers, raw media, or sensitive session data in source control or logs.
- Run `dotnet build` and `dotnet test` before handing work off.
