using ChatLab.Web.Components;
using ChatLab.Application;
using ChatLab.Domain;
using ChatLab.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddChatLabSqlite(builder.Configuration.GetConnectionString("ChatLab")!);
builder.Services.AddCors(options => options.AddPolicy("collector", policy => policy
    .WithOrigins(builder.Configuration.GetSection("Collector:AllowedOrigins").Get<string[]>() ?? ["https://localhost:7252", "http://localhost:5198"])
    .WithMethods("POST")
    .WithHeaders("content-type")));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ChatLabDbContext>();
    await db.Database.EnsureCreatedAsync();
    await db.EnsureTelemetrySchemaAsync();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();
app.UseCors("collector");

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

var telemetry = app.MapGroup("/api").DisableAntiforgery();
telemetry.MapPost("/sessions", async (CreateResearchSessionRequest request, IResearchSessionRepository sessions, ILogger<Program> logger, CancellationToken cancellationToken) =>
{
    if (string.IsNullOrWhiteSpace(request.Label))
        return Results.ValidationProblem(new Dictionary<string, string[]> { ["label"] = ["A session label is required."] });

    var session = new ResearchSession(request.Label, DateTimeOffset.UtcNow, request.Notes);
    await sessions.AddAsync(session, cancellationToken);
    logger.LogInformation("Created consented WebRTC research session {ResearchSessionId}", session.Id);
    return Results.Created($"/api/sessions/{session.Id}", new { session.Id });
});

telemetry.MapPost("/sessions/{sessionId:guid}/webrtc-samples", async (Guid sessionId, WebRtcTelemetrySampleDto input, IResearchSessionRepository sessions, ILogger<Program> logger, CancellationToken cancellationToken) =>
{
    if (await sessions.GetAsync(sessionId, cancellationToken) is null) return Results.NotFound();

    var sample = WebRtcTelemetryNormalizer.Normalize(sessionId, input);
    await sessions.AddSampleAsync(sample, cancellationToken);
    var anomalies = WebRtcAnomalyRules.Evaluate(sample);
    logger.LogInformation("Stored WebRTC sample for session {ResearchSessionId}: connection={ConnectionState}, ice={IceConnectionState}, inboundBytes={InboundBytes}, outboundBytes={OutboundBytes}, anomalies={AnomalyCount}", sessionId, sample.ConnectionState, sample.IceConnectionState, sample.InboundBytes, sample.OutboundBytes, anomalies.Count);
    if (anomalies.Count > 0)
        logger.LogWarning("WebRTC anomalies for session {ResearchSessionId}: {AnomalyCodes}", sessionId, string.Join(',', anomalies.Select(x => x.Code)));
    return Results.Accepted($"/api/sessions/{sessionId}/webrtc-samples");
});

app.Run();

public sealed record CreateResearchSessionRequest(string? Label, string? Notes);
