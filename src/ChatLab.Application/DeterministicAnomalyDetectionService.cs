using ChatLab.Domain;

namespace ChatLab.Application;

/// <summary>Sequence-aware, deterministic telemetry rules. Findings describe observations, never user intent.</summary>
public sealed class DeterministicAnomalyDetectionService : IAnomalyDetectionService
{
    private static readonly TimeSpan EarlyMediaLead = TimeSpan.FromSeconds(3);

    public IReadOnlyList<AnomalyEvent> Detect(WebRtcSample current, IReadOnlyList<WebRtcSample> precedingSamples)
    {
        var ordered = precedingSamples.OrderBy(x => x.CapturedAtUtc).ToList();
        var prior = ordered.LastOrDefault();
        var findings = new List<AnomalyEvent>();
        var inboundIncreasing = prior is not null && current.InboundBytes > prior.InboundBytes;
        var framesDecodedIncreasing = prior?.FramesDecoded is not null && current.FramesDecoded > prior.FramesDecoded;

        if (current.ConnectionState == "connected" && current.UiState == "searching" && inboundIncreasing)
            findings.Add(Create(current, AnomalyType.MediaWhileUiSearching, AnomalySeverity.Warning,
                $"uiState=searching; connectionState=connected; bytesReceived increased from {prior!.InboundBytes} to {current.InboundBytes}."));

        if (current.ConnectionState == "connected" && current.UiState == "disconnected" && framesDecodedIncreasing)
            findings.Add(Create(current, AnomalyType.MediaWhileUiDisconnected, AnomalySeverity.Warning,
                $"uiState=disconnected; connectionState=connected; framesDecoded increased from {prior!.FramesDecoded} to {current.FramesDecoded}."));

        if (ordered.Count >= 2 && ordered[^2].IceConnectionState == "connected" &&
            prior?.IceConnectionState == "checking" && current.IceConnectionState == "connected")
            findings.Add(Create(current, AnomalyType.IceRestart, AnomalySeverity.Informational,
                "iceConnectionState sequence observed: connected → checking → connected."));

        if (inboundIncreasing && current.ConnectionState == "connected" &&
            current.UiState != "connected" &&
            !ordered.Any(x => x.UiState == "connected") &&
            current.CapturedAtUtc - ordered.FirstOrDefault()?.CapturedAtUtc >= EarlyMediaLead)
            findings.Add(Create(current, AnomalyType.EarlyMedia, AnomalySeverity.Warning,
                $"bytesReceived increased from {prior!.InboundBytes} to {current.InboundBytes} at least {EarlyMediaLead.TotalSeconds:0} seconds before any visible connected state."));

        return findings;
    }

    private static AnomalyEvent Create(WebRtcSample sample, AnomalyType type, AnomalySeverity severity, string evidence) =>
        new(sample.ResearchSessionId, sample.Id, sample.CapturedAtUtc, type, severity, evidence);
}
