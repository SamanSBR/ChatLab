namespace ChatLab.Domain;

/// <summary>An explainable, deterministic observation derived from consented telemetry.</summary>
public sealed class AnomalyEvent
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid ResearchSessionId { get; private set; }
    public Guid WebRtcSampleId { get; private set; }
    public DateTimeOffset OccurredAtUtc { get; private set; }
    public AnomalyType Type { get; private set; }
    public AnomalySeverity Severity { get; private set; }
    public string Evidence { get; private set; } = string.Empty;

    private AnomalyEvent() { }

    public AnomalyEvent(Guid researchSessionId, Guid webRtcSampleId, DateTimeOffset occurredAtUtc,
        AnomalyType type, AnomalySeverity severity, string evidence)
    {
        if (researchSessionId == Guid.Empty) throw new ArgumentException("Session id is required.", nameof(researchSessionId));
        if (webRtcSampleId == Guid.Empty) throw new ArgumentException("Sample id is required.", nameof(webRtcSampleId));
        if (string.IsNullOrWhiteSpace(evidence)) throw new ArgumentException("Evidence is required.", nameof(evidence));

        ResearchSessionId = researchSessionId;
        WebRtcSampleId = webRtcSampleId;
        OccurredAtUtc = occurredAtUtc;
        Type = type;
        Severity = severity;
        Evidence = evidence.Trim();
    }
}
