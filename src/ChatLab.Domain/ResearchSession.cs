namespace ChatLab.Domain;

/// <summary>One consented, controlled research observation session.</summary>
public sealed class ResearchSession
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public DateTimeOffset StartedAtUtc { get; private set; }
    public DateTimeOffset? EndedAtUtc { get; private set; }
    public string Label { get; private set; } = string.Empty;
    public string? Notes { get; private set; }
    public ICollection<WebRtcSample> WebRtcSamples { get; private set; } = new List<WebRtcSample>();

    private ResearchSession() { }

    public ResearchSession(string label, DateTimeOffset startedAtUtc, string? notes = null)
    {
        if (string.IsNullOrWhiteSpace(label))
            throw new ArgumentException("A research session needs a label.", nameof(label));

        Label = label.Trim();
        StartedAtUtc = startedAtUtc;
        Notes = notes?.Trim();
    }

    public void End(DateTimeOffset endedAtUtc)
    {
        if (endedAtUtc < StartedAtUtc)
            throw new ArgumentOutOfRangeException(nameof(endedAtUtc), "A session cannot end before it starts.");

        EndedAtUtc = endedAtUtc;
    }
}
