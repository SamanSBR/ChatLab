using ChatLab.Domain;

namespace ChatLab.Application;

public interface IResearchSessionRepository
{
    Task<IReadOnlyList<ResearchSession>> GetRecentAsync(int take, CancellationToken cancellationToken = default);
    Task<int> CountAsync(CancellationToken cancellationToken = default);
    Task AddAsync(ResearchSession session, CancellationToken cancellationToken = default);
    Task<ResearchSession?> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddSampleAsync(WebRtcSample sample, CancellationToken cancellationToken = default);
    Task<WebRtcSample?> GetLatestSampleAsync(CancellationToken cancellationToken = default);
}
