using ChatLab.Domain;

namespace ChatLab.Application;

/// <summary>Converts browser units and optional fields into the durable domain representation.</summary>
public static class WebRtcTelemetryNormalizer
{
    public static WebRtcSample Normalize(Guid sessionId, WebRtcTelemetrySampleDto source) => new(
        sessionId,
        source.CapturedAtUtc == default ? DateTimeOffset.UtcNow : source.CapturedAtUtc,
        source.ConnectionState ?? "unknown",
        source.IceConnectionState ?? "unknown",
        source.SignalingState ?? "unknown",
        NonNegative(source.BytesReceived), NonNegative(source.BytesSent),
        NonNegativeNullable(source.PacketsReceived),
        null, null,
        ToMilliseconds(source.CurrentRoundTripTimeSeconds), ToMilliseconds(source.JitterSeconds), NonNegativeNullable(source.PacketsLost),
        NonNegativeNullable(source.FramesReceived), NonNegativeNullable(source.FramesDecoded), NonNegativeNullable(source.PacketsSent),
        NonNegativeNullable(source.FramesEncoded), NonNegativeNullable(source.FramesSent),
        ToKbps(source.AvailableIncomingBitrateBps), ToKbps(source.AvailableOutgoingBitrateBps),
        source.InboundCodec, source.OutboundCodec, source.LocalCandidateType, source.RemoteCandidateType, source.SelectedCandidatePairId);

    private static long NonNegative(long? value) => value is > 0 ? value.Value : 0;
    private static long? NonNegativeNullable(long? value) => value is null ? null : Math.Max(0, value.Value);
    private static double? ToMilliseconds(double? seconds) => seconds is null || seconds < 0 ? null : seconds * 1000d;
    private static double? ToKbps(double? bitsPerSecond) => bitsPerSecond is null || bitsPerSecond < 0 ? null : bitsPerSecond / 1000d;
}
