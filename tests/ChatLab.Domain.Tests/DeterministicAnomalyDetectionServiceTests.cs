using ChatLab.Application;
using ChatLab.Domain;

namespace ChatLab.Domain.Tests;

public sealed class DeterministicAnomalyDetectionServiceTests
{
    private readonly DeterministicAnomalyDetectionService service = new();
    private readonly Guid sessionId = Guid.NewGuid();
    private readonly DateTimeOffset start = new(2026, 9, 24, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Detect_EmitsMediaWhileUiSearching_WhenConnectedInboundBytesIncrease()
    {
        var prior = Sample(0, "connected", "connected", "searching", 100, 1);
        var result = service.Detect(Sample(1, "connected", "connected", "searching", 200, 1), [prior]);

        var anomaly = Assert.Single(result);
        Assert.Equal(AnomalyType.MediaWhileUiSearching, anomaly.Type);
        Assert.Equal(AnomalySeverity.Warning, anomaly.Severity);
        Assert.Contains("bytesReceived increased", anomaly.Evidence);
    }

    [Fact]
    public void Detect_EmitsMediaWhileUiDisconnected_WhenFramesDecodedIncrease()
    {
        var prior = Sample(0, "connected", "connected", "disconnected", 100, 4);
        var result = service.Detect(Sample(1, "connected", "connected", "disconnected", 100, 5), [prior]);

        var anomaly = Assert.Single(result);
        Assert.Equal(AnomalyType.MediaWhileUiDisconnected, anomaly.Type);
        Assert.Equal(AnomalySeverity.Warning, anomaly.Severity);
    }

    [Fact]
    public void Detect_EmitsIceRestart_ForConnectedCheckingConnectedSequence()
    {
        var connected = Sample(0, "connected", "connected", "connected", 0, 0);
        var checking = Sample(1, "connected", "checking", "connected", 0, 0);
        var result = service.Detect(Sample(2, "connected", "connected", "connected", 0, 0), [connected, checking]);

        var anomaly = Assert.Single(result);
        Assert.Equal(AnomalyType.IceRestart, anomaly.Type);
        Assert.Equal(AnomalySeverity.Informational, anomaly.Severity);
    }

    [Fact]
    public void Detect_EmitsEarlyMedia_WhenMediaPrecedesVisibleConnectionByThreeSeconds()
    {
        var prior = Sample(0, "connected", "connected", "searching", 100, 0);
        var result = service.Detect(Sample(3, "connected", "connected", "searching", 200, 0), [prior]);

        var anomaly = Assert.Single(result, x => x.Type == AnomalyType.EarlyMedia);
        Assert.Equal(AnomalyType.EarlyMedia, anomaly.Type);
        Assert.Equal(AnomalySeverity.Warning, anomaly.Severity);
    }

    private WebRtcSample Sample(int seconds, string connection, string ice, string ui, long bytes, long frames) =>
        new(sessionId, start.AddSeconds(seconds), connection, ice, "stable", bytes, 0, ui, framesDecoded: frames);
}
