namespace ChatLab.Domain;

/// <summary>A passive point-in-time snapshot of browser-supplied WebRTC telemetry.</summary>
public sealed class WebRtcSample
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid ResearchSessionId { get; private set; }
    public DateTimeOffset CapturedAtUtc { get; private set; }
    public string ConnectionState { get; private set; } = "unknown";
    public string IceConnectionState { get; private set; } = "unknown";
    public string SignalingState { get; private set; } = "unknown";
    public string UiState { get; private set; } = "unknown";
    public long InboundBytes { get; private set; }
    public long OutboundBytes { get; private set; }
    public long? PacketsReceived { get; private set; }
    public double? InboundBitrateKbps { get; private set; }
    public double? OutboundBitrateKbps { get; private set; }
    public double? RoundTripTimeMs { get; private set; }
    public double? JitterMs { get; private set; }
    public long? PacketsLost { get; private set; }
    public long? FramesReceived { get; private set; }
    public long? FramesDecoded { get; private set; }
    public long? PacketsSent { get; private set; }
    public long? FramesEncoded { get; private set; }
    public long? FramesSent { get; private set; }
    public double? AvailableIncomingBitrateKbps { get; private set; }
    public double? AvailableOutgoingBitrateKbps { get; private set; }
    public string? InboundCodec { get; private set; }
    public string? OutboundCodec { get; private set; }
    public string? LocalCandidateType { get; private set; }
    public string? RemoteCandidateType { get; private set; }
    public string? SelectedCandidatePairId { get; private set; }

    private WebRtcSample() { }

    public WebRtcSample(Guid researchSessionId, DateTimeOffset capturedAtUtc,
        string connectionState, string iceConnectionState, string signalingState, long inboundBytes, long outboundBytes,
        string? uiState = null,
        long? packetsReceived = null,
        double? inboundBitrateKbps = null, double? outboundBitrateKbps = null,
        double? roundTripTimeMs = null, double? jitterMs = null, long? packetsLost = null,
        long? framesReceived = null, long? framesDecoded = null, long? packetsSent = null,
        long? framesEncoded = null, long? framesSent = null,
        double? availableIncomingBitrateKbps = null, double? availableOutgoingBitrateKbps = null,
        string? inboundCodec = null, string? outboundCodec = null,
        string? localCandidateType = null, string? remoteCandidateType = null, string? selectedCandidatePairId = null)
    {
        if (researchSessionId == Guid.Empty) throw new ArgumentException("Session id is required.", nameof(researchSessionId));
        if (inboundBytes < 0 || outboundBytes < 0) throw new ArgumentOutOfRangeException(nameof(inboundBytes), "Byte counts cannot be negative.");

        ResearchSessionId = researchSessionId;
        CapturedAtUtc = capturedAtUtc;
        ConnectionState = NormalizeState(connectionState);
        IceConnectionState = NormalizeState(iceConnectionState);
        SignalingState = NormalizeState(signalingState);
        UiState = NormalizeState(uiState ?? "unknown");
        InboundBytes = inboundBytes;
        OutboundBytes = outboundBytes;
        PacketsReceived = packetsReceived;
        InboundBitrateKbps = inboundBitrateKbps;
        OutboundBitrateKbps = outboundBitrateKbps;
        RoundTripTimeMs = roundTripTimeMs;
        JitterMs = jitterMs;
        PacketsLost = packetsLost;
        FramesReceived = framesReceived;
        FramesDecoded = framesDecoded;
        PacketsSent = packetsSent;
        FramesEncoded = framesEncoded;
        FramesSent = framesSent;
        AvailableIncomingBitrateKbps = availableIncomingBitrateKbps;
        AvailableOutgoingBitrateKbps = availableOutgoingBitrateKbps;
        InboundCodec = inboundCodec;
        OutboundCodec = outboundCodec;
        LocalCandidateType = localCandidateType;
        RemoteCandidateType = remoteCandidateType;
        SelectedCandidatePairId = selectedCandidatePairId;
    }

    private static string NormalizeState(string value) => string.IsNullOrWhiteSpace(value) ? "unknown" : value.Trim().ToLowerInvariant();
}
