using ChatLab.Application;
using ChatLab.Domain;

namespace ChatLab.Domain.Tests;

public sealed class WebRtcTelemetryTests
{
    [Fact]
    public void Normalize_ConvertsBrowserUnitsAndGracefullyHandlesUnavailableFields()
    {
        var capturedAt = new DateTimeOffset(2026, 9, 24, 12, 0, 0, TimeSpan.Zero);
        var dto = new WebRtcTelemetrySampleDto(capturedAt, "CONNECTED", "completed", "stable", 1200, 900, null, -1, 42, null, .015, 7, 3, 2, .25, 1_500_000, null, "host", "relay", "pair-1", "audio/opus", null);

        var sample = WebRtcTelemetryNormalizer.Normalize(Guid.NewGuid(), dto);

        Assert.Equal("connected", sample.ConnectionState);
        Assert.Equal(15, sample.JitterMs);
        Assert.Equal(250, sample.RoundTripTimeMs);
        Assert.Equal(1500, sample.AvailableIncomingBitrateKbps);
        Assert.Null(sample.PacketsReceived);
        Assert.Equal(0, sample.PacketsLost);
        Assert.Null(sample.AvailableOutgoingBitrateKbps);
        Assert.Equal("pair-1", sample.SelectedCandidatePairId);
    }

    [Fact]
    public void AnomalyRules_FlagFailedConnectionAndPoorNetworkConditions()
    {
        var sample = new WebRtcSample(Guid.NewGuid(), DateTimeOffset.UtcNow, "failed", "disconnected", "stable", 0, 0,
            roundTripTimeMs: 1001, jitterMs: 101);

        var codes = WebRtcAnomalyRules.Evaluate(sample).Select(x => x.Code).ToArray();

        Assert.Equal(["connection-state", "ice-state", "high-rtt", "high-jitter"], codes);
    }
}
