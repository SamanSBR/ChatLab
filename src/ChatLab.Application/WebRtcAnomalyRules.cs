using ChatLab.Domain;

namespace ChatLab.Application;

public sealed record WebRtcAnomaly(string Code, string Message);

/// <summary>Small, explainable rules for highlighting samples that merit review.</summary>
public static class WebRtcAnomalyRules
{
    public static IReadOnlyList<WebRtcAnomaly> Evaluate(WebRtcSample sample)
    {
        var findings = new List<WebRtcAnomaly>();
        if (sample.ConnectionState is "failed" or "disconnected") findings.Add(new("connection-state", $"Peer connection is {sample.ConnectionState}."));
        if (sample.IceConnectionState is "failed" or "disconnected") findings.Add(new("ice-state", $"ICE connection is {sample.IceConnectionState}."));
        if (sample.RoundTripTimeMs is > 1000) findings.Add(new("high-rtt", "Round-trip time exceeds 1000 ms."));
        if (sample.JitterMs is > 100) findings.Add(new("high-jitter", "Inbound jitter exceeds 100 ms."));
        return findings;
    }
}
