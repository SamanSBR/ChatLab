using ChatLab.Domain;

namespace ChatLab.Domain.Tests;

public sealed class ResearchSessionTests
{
    [Fact]
    public void End_RejectsTimestampBeforeSessionStart()
    {
        var start = new DateTimeOffset(2026, 9, 24, 10, 0, 0, TimeSpan.Zero);
        var session = new ResearchSession("controlled baseline", start);

        Assert.Throws<ArgumentOutOfRangeException>(() => session.End(start.AddSeconds(-1)));
    }

    [Fact]
    public void Sample_NormalizesConnectionStates()
    {
        var sample = new WebRtcSample(Guid.NewGuid(), DateTimeOffset.UtcNow, " CONNECTED ", "Checking", 0, 0);

        Assert.Equal("connected", sample.ConnectionState);
        Assert.Equal("checking", sample.IceConnectionState);
    }
}
