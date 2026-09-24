using ChatLab.Application;
using ChatLab.Domain;
using Microsoft.EntityFrameworkCore;

namespace ChatLab.Infrastructure;

public sealed class SqliteResearchSessionRepository(ChatLabDbContext db) : IResearchSessionRepository
{
    public async Task<IReadOnlyList<ResearchSession>> GetRecentAsync(int take, CancellationToken cancellationToken = default)
    {
        // SQLite cannot translate DateTimeOffset ordering; sessions are local research data,
        // so order after materialization until a provider-specific timestamp conversion is added.
        var sessions = await db.ResearchSessions.AsNoTracking().Include(x => x.WebRtcSamples).ToListAsync(cancellationToken);
        return sessions.OrderByDescending(s => s.StartedAtUtc).Take(take).ToList();
    }

    public Task<int> CountAsync(CancellationToken cancellationToken = default) => db.ResearchSessions.CountAsync(cancellationToken);

    public async Task AddAsync(ResearchSession session, CancellationToken cancellationToken = default)
    {
        db.ResearchSessions.Add(session);
        await db.SaveChangesAsync(cancellationToken);
    }

    public Task<ResearchSession?> GetAsync(Guid id, CancellationToken cancellationToken = default) =>
        db.ResearchSessions.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task AddSampleAsync(WebRtcSample sample, CancellationToken cancellationToken = default)
    {
        db.WebRtcSamples.Add(sample);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<WebRtcSample?> GetLatestSampleAsync(CancellationToken cancellationToken = default)
    {
        var samples = await db.WebRtcSamples.AsNoTracking().ToListAsync(cancellationToken);
        return samples.OrderByDescending(x => x.CapturedAtUtc).FirstOrDefault();
    }

    public async Task<IReadOnlyList<WebRtcSample>> GetSamplesAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        var samples = await db.WebRtcSamples.AsNoTracking().Where(x => x.ResearchSessionId == sessionId).ToListAsync(cancellationToken);
        return samples.OrderBy(x => x.CapturedAtUtc).ToList();
    }

    public async Task AddAnomaliesAsync(IEnumerable<AnomalyEvent> anomalies, CancellationToken cancellationToken = default)
    {
        var items = anomalies.ToList();
        if (items.Count == 0) return;
        db.AnomalyEvents.AddRange(items);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AnomalyEvent>> GetAnomaliesAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        var events = await db.AnomalyEvents.AsNoTracking().Where(x => x.ResearchSessionId == sessionId).ToListAsync(cancellationToken);
        return events.OrderBy(x => x.OccurredAtUtc).ToList();
    }
}
