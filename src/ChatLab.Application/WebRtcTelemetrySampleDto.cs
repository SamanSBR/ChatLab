namespace ChatLab.Application;

/// <summary>Typed, transport-safe telemetry emitted by the browser collector.</summary>
public sealed record WebRtcTelemetrySampleDto(
    DateTimeOffset CapturedAtUtc,
    string? ConnectionState,
    string? IceConnectionState,
    string? SignalingState,
    string? UiState,
    long? BytesReceived,
    long? BytesSent,
    long? PacketsReceived,
    long? PacketsLost,
    long? FramesReceived,
    long? FramesDecoded,
    double? JitterSeconds,
    long? PacketsSent,
    long? FramesEncoded,
    long? FramesSent,
    double? CurrentRoundTripTimeSeconds,
    double? AvailableIncomingBitrateBps,
    double? AvailableOutgoingBitrateBps,
    string? LocalCandidateType,
    string? RemoteCandidateType,
    string? SelectedCandidatePairId,
    string? InboundCodec,
    string? OutboundCodec);
