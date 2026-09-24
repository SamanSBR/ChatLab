using ChatLab.Application;
using ChatLab.Domain;
using Microsoft.EntityFrameworkCore;

namespace ChatLab.Infrastructure;

public sealed class SqliteResearchSessionRepository(ChatLabDbContext db) : IResearchSessionRepository
{
    public async Task<IReadOnlyList<ResearchSession>> GetRecentAsync(int take, CancellationToken cancellationToken = default) =>
        await db.ResearchSessions.AsNoTracking().OrderByDescending(s => s.StartedAtUtc).Take(take).ToListAsync(cancellationToken);

    public Task<int> CountAsync(CancellationToken cancellationToken = default) => db.ResearchSessions.CountAsync(cancellationToken);

    public async Task AddAsync(ResearchSession session, CancellationToken cancellationToken = default)
    {
        db.ResearchSessions.Add(session);
        await db.SaveChangesAsync(cancellationToken);
    }
}
